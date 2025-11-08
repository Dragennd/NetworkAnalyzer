using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal class IPScannerService : IIPScannerService, INotifyPropertyChanged
    {
        #region Properties
        public event PropertyChangedEventHandler? PropertyChanged;

        // Contains all active subnets as calculated by the available NICs on the device
        private List<IPv4Info> ActiveSubnets { get; set; }

        // Contains the processed subnet data defined by the user in SubnetsToScan
        private IPv4Info UserDefinedSubnet { get; set; }

        // Contains ReportID for this session
        private string ReportID { get; set; }

        // Contains the datetime the scan was started
        private string StartTime { get; set; }

        // Contains user input for the subnets to scan by the network scan
        private string _subnetsToScan = string.Empty;
        public string SubnetsToScan
        {
            get => _subnetsToScan;
            set
            {
                if (_subnetsToScan != value)
                {
                    _subnetsToScan = value;
                }
            }
        }

        // Contains the amount of time the scan was active
        private string _scanDuration = "00:00.000";
        public string ScanDuration
        {
            get => _scanDuration;
            set
            {
                if (_scanDuration != value)
                {
                    _scanDuration = value;
                }
            }
        }

        // Contains the overall amount of addresses to be scanned
        private int _totalAddressCount = 0;

        // Contains the amount of addresses which returned data and are considered active
        private int _totalActiveAddresses = 0;

        // Contains the amount of addresses which failed to return data and are considered inactive
        private int _totalInactiveAddresses = 0;

        private readonly SemaphoreSlim _manufacturerSemaphore = new(1);

        private readonly ISubnetHandler _subnetHandler;

        private readonly IMACAddressHandler _macAddressHandler;

        private readonly IRDPHandler _rdpHandler;

        private readonly ISSHHandler _sshHandler;

        private readonly ISMBHandler _smbHandler;

        private readonly IDatabaseHandler _dbHandler;

        private readonly IDNSHandler _dnsHandler;

        private readonly IIPScannerController _ipScannerController;
        #endregion Properties

        public IPScannerService(ISubnetHandler subnetHandler,
                                IMACAddressHandler macAddressHandler,
                                IRDPHandler rdpHandler,
                                ISSHHandler sshHandler,
                                ISMBHandler smbHandler,
                                IDatabaseHandler dbHandler,
                                IDNSHandler dnsHandler,
                                IIPScannerController ipScannerController)
        {
            ActiveSubnets = new();
            
            _subnetHandler = subnetHandler;
            _macAddressHandler = macAddressHandler;
            _rdpHandler = rdpHandler;
            _sshHandler = sshHandler;
            _smbHandler = smbHandler;
            _dbHandler = dbHandler;
            _dnsHandler = dnsHandler;
            _ipScannerController = ipScannerController;
        }

        public async Task StartScanAsync(bool isAutoChecked)
        {
            ResetStatistics();

            StartTime = DateTime.Now.ToString("G");
            ReportID = GenerateNewGUID();
            await _dbHandler.NewIPScannerReportAsync(new IPScannerReports()
            {
                ReportID = ReportID,
                TotalScannableIPs = _totalAddressCount,
                TotalActiveIPs = _totalActiveAddresses,
                TotalInactiveIPs = _totalInactiveAddresses,
                TotalDuration = ScanDuration,
                CreatedWhen = StartTime,
                ReportMode = ReportMode.IPScanner
            });

            if (!isAutoChecked) // If Manual Mode is set, perform these checks
            {
                UserDefinedSubnet = new IPv4Info(_subnetsToScan, isAutoChecked);

                if (UserDefinedSubnet.IsError == true)
                {
                    // To-do: Create a method to send the error back to the view model to then be displayed
                    return;
                }

                var info = await _subnetHandler.CalculateNetworkAndBroadcastAddressesAsync(UserDefinedSubnet.IPv4Address, UserDefinedSubnet.SubnetMask);

                UserDefinedSubnet.NetworkAddress = info._networkAddress;
                UserDefinedSubnet.BroadcastAddress = info._broadcastAddress;
            }
            else // If Auto Mode is set, perform these checks
            {
                ActiveSubnets = await _subnetHandler.GenerateListOfActiveSubnetsAsync();
            }

            _ipScannerController.SendUpdateScanStatusRequest("SCANNING");
            await ProcessActiveSubnetsAsync(isAutoChecked);
            await _dbHandler.UpdateIPScannerReportAsync(new IPScannerReports()
            {
                ReportID = ReportID,
                TotalScannableIPs = _totalAddressCount,
                TotalActiveIPs = _totalActiveAddresses,
                TotalInactiveIPs = _totalInactiveAddresses,
                TotalDuration = ScanDuration,
                CreatedWhen = StartTime,
                ReportMode = ReportMode.IPScanner
            });
            _ipScannerController.SendUpdateScanStatusRequest("IDLE");
        }

        #region Private Methods
        private async Task ProcessActiveSubnetsAsync(bool isAutoChecked)
        {
            int[] lowerBound = new int[4];
            int[] upperBound = new int[4];

            if (isAutoChecked)
            {
                foreach (var subnet in ActiveSubnets)
                {
                    lowerBound = subnet.NetworkAddress.Split('.').Select(int.Parse).ToArray();
                    upperBound = subnet.BroadcastAddress.Split('.').Select(int.Parse).ToArray();

                    foreach (var ip in GenerateIPs(lowerBound, upperBound).Chunk(200))
                    {
                        var tasks = ip.Select(ip => ProcessIPAddressAsync(ip));
                        await Task.WhenAll(tasks);

                        await Task.Yield();
                    }
                }

                lowerBound = new int[4];
                upperBound = new int[4];
            }
            else
            {
                if (UserDefinedSubnet.IsIPv4Range)
                {
                    lowerBound = UserDefinedSubnet.LowerRange.Split('.').Select(int.Parse).ToArray();
                    upperBound = UserDefinedSubnet.UpperRange.Split('.').Select(int.Parse).ToArray();
                }
                else if (UserDefinedSubnet.IsIPv4WithCIDR || UserDefinedSubnet.IsIPv4WithMask)
                {
                    lowerBound = UserDefinedSubnet.NetworkAddress.Split('.').Select(int.Parse).ToArray();
                    upperBound = UserDefinedSubnet.BroadcastAddress.Split('.').Select(int.Parse).ToArray();
                }

                foreach (var ip in GenerateIPs(lowerBound, upperBound).Chunk(100))
                {
                    var tasks = ip.Select(ip => ProcessIPAddressAsync(ip));
                    await Task.WhenAll(tasks);

                    await Task.Yield();
                }

                lowerBound = new int[4];
                upperBound = new int[4];
            }
        }

        private IEnumerable<string> GenerateIPs(int[] lowerBound, int[] upperBound)
        {
            // Loops through the provided bounds for the first octet of the IP Address to be generated
            for (int h = lowerBound[0]; h <= upperBound[0]; h++)
            {
                // Loops through the provided bounds for the second octet of the IP Address to be generated
                for (int i = lowerBound[1]; i <= upperBound[1]; i++)
                {
                    // Loops through the provided bounds for the third octet of the IP Address to be generated
                    for (int j = lowerBound[2]; j <= upperBound[2]; j++)
                    {
                        // Loops through the provided bounds for the fourth octet of the IP Address to be generated
                        for (int k = lowerBound[3]; k <= upperBound[3]; k++)
                        {
                            yield return $"{h}.{i}.{j}.{k}";
                        }
                    }
                }
            }
        }

        private async Task ProcessIPAddressAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var pingResult = await ping.SendPingAsync(ipAddress, 2000);

                    if (pingResult.Status == IPStatus.Success)
                    {
                        var mac = await _macAddressHandler.GetMACAddressAsync(pingResult.Address.ToString());
                        var results = await NewIPScannerDataAsync(pingResult.Address.ToString(), mac);

                        _ipScannerController.SendAddScanResultsRequest(results);
                        await _dbHandler.NewIPScannerReportEntryAsync(results);

                        Interlocked.Increment(ref _totalActiveAddresses);
                        _ipScannerController.SendUpdateTotalActiveAddressesRequest(_totalActiveAddresses);
                    }
                    else
                    {
                        Interlocked.Increment(ref _totalInactiveAddresses);
                        _ipScannerController.SendUpdateTotalInactiveAddressesRequest(_totalInactiveAddresses);
                    }
                }
            }
            catch (Exception)
            {
                Interlocked.Increment(ref _totalInactiveAddresses);
                _ipScannerController.SendUpdateTotalInactiveAddressesRequest(_totalInactiveAddresses);
            }
            finally
            {
                Interlocked.Increment(ref _totalAddressCount);
                _ipScannerController.SendUpdateTotalAddressCountRequest(_totalAddressCount);
            }
        }

        private async Task<IPScannerData> NewIPScannerDataAsync(string ipAddress, string macAddress)
        {
            var activeIP = new IPScannerData();

            activeIP.ReportID = ReportID;
            activeIP.IPAddress = ipAddress;
            activeIP.MACAddress = macAddress.ToUpper();
            activeIP.Name = await _dnsHandler.GetDeviceNameAsync(ipAddress);

            await _manufacturerSemaphore.WaitAsync();
            activeIP.Manufacturer = await _macAddressHandler.GetManufacturerAsync(macAddress);
            await Task.Delay(600);
            _manufacturerSemaphore.Release();

            activeIP.RDPEnabled = await _rdpHandler.ScanRDPPortAsync(ipAddress);
            activeIP.SMBEnabled = await _smbHandler.ScanSMBPortAsync(ipAddress);
            activeIP.SSHEnabled = await _sshHandler.ScanSSHPortAsync(ipAddress);

            return activeIP;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResetStatistics()
        {
            ActiveSubnets.Clear();
            StartTime = string.Empty;
            UserDefinedSubnet = null;
            _totalAddressCount = 0;
            _totalActiveAddresses = 0;
            _totalInactiveAddresses = 0;
        }

        private string GenerateNewGUID() => Guid.NewGuid().ToString();
        #endregion Private Methods
    }
}
