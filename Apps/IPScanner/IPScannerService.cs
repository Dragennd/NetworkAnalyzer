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
                    OnPropertyChanged(nameof(SubnetsToScan));
                }
            }
        }

        // Contains the current status of the network scan
        private string _scanStatus = string.Empty;
        public string ScanStatus
        {
            get => _scanStatus;
            set
            {
                if (_scanStatus != value)
                {
                    _scanStatus = value;
                    OnPropertyChanged(nameof(ScanStatus));
                }
            }
        }

        // Contains the amount of time the scan was active
        private string _scanDuration = string.Empty;
        public string ScanDuration
        {
            get => _scanDuration;
            set
            {
                if (_scanDuration != value)
                {
                    _scanDuration = value;
                    OnPropertyChanged(nameof(ScanDuration));
                }
            }
        }

        // Contains the overall amount of addresses to be scanned
        private int _totalAddressCount = 0;
        public int TotalAddressCount
        {
            get => _totalAddressCount;
            set
            {
                if (_totalAddressCount != value)
                {
                    _totalAddressCount = value;
                    OnPropertyChanged(nameof(TotalAddressCount));
                }
            }
        }

        // Contains the amount of addresses which returned data and are considered active
        private int _totalActiveAddresses = 0;
        public int TotalActiveAddresses
        {
            get => _totalActiveAddresses;
            set
            {
                if (_totalActiveAddresses != value)
                {
                    _totalActiveAddresses = value;
                    OnPropertyChanged(nameof(TotalActiveAddresses));
                }
            }
        }

        // Contains the amount of addresses which failed to return data and are considered inactive
        private int _totalInactiveAddresses = 0;
        public int TotalInactiveAddresses
        {
            get => _totalInactiveAddresses;
            set
            {
                if (_totalInactiveAddresses != value)
                {
                    _totalInactiveAddresses = value;
                    OnPropertyChanged(nameof(TotalInactiveAddresses));
                }
            }
        }

        private readonly SemaphoreSlim _resultsSemaphore = new(300);

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
            if (!isAutoChecked) // If Manual Mode is set, perform these checks
            {
                UserDefinedSubnet = new IPv4Info(SubnetsToScan, isAutoChecked);

                if (UserDefinedSubnet.IsError == true)
                {
                    // To-do: Create a method to send the error back to the view model to then be displayed
                    return;
                }
            }
            else // If Auto Mode is set, perform these checks
            {
                ActiveSubnets = await _subnetHandler.GenerateListOfActiveSubnetsAsync();
            }

            await ProcessActiveSubnetsAsync(isAutoChecked);
        }

        #region Private Methods
        private async Task ProcessActiveSubnetsAsync(bool isAutoChecked)
        {
            var tasks = new List<Task>();

            int[] lowerBound = new int[4];
            int[] upperBound = new int[4];

            if (isAutoChecked)
            {
                foreach (var subnet in ActiveSubnets)
                {
                    lowerBound = subnet.NetworkAddress.Split('.').Select(int.Parse).ToArray();
                    upperBound = subnet.BroadcastAddress.Split('.').Select(int.Parse).ToArray();

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
                                    var task = Task.Run(async () =>
                                    {
                                        await _resultsSemaphore.WaitAsync();
                                        await ProcessIPAddressAsync(await GenerateIPAddressAsync(h, i, j, k));
                                        _resultsSemaphore.Release();
                                    });

                                    tasks.Add(task);
                                }
                            }
                        }
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
                                var task = Task.Run(async () =>
                                {
                                    await _resultsSemaphore.WaitAsync();
                                    await ProcessIPAddressAsync(await GenerateIPAddressAsync(h, i, j, k));
                                    _resultsSemaphore.Release();
                                });

                                tasks.Add(task);
                            }
                        }
                    }
                }

                lowerBound = new int[4];
                upperBound = new int[4];
            }

            await Task.WhenAll(tasks);
        }

        private async Task<string> GenerateIPAddressAsync(int octet1, int octet2, int octet3, int octet4)
        {
            string[] ipArray = new string[4];

            ipArray[0] = octet1.ToString();
            ipArray[1] = octet2.ToString();
            ipArray[2] = octet3.ToString();
            ipArray[3] = octet4.ToString();

            // Return the re-combined IP Address as a string
            return await Task.FromResult(string.Join(".", ipArray));
        }

        private async Task ProcessIPAddressAsync(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var pingResult = await ping.SendPingAsync(ipAddress, 1000);

                    if (pingResult.Status == IPStatus.Success)
                    {
                        var mac = await _macAddressHandler.GetMACAddressAsync(pingResult.Address.ToString());
                        var results = await NewIPScannerDataAsync(pingResult.Address.ToString(), mac);

                        _ipScannerController.SendAddScanResultsRequest(results);
                        await _dbHandler.NewIPScannerReportEntryAsync(results);

                        TotalActiveAddresses++;
                    }
                    else
                    {
                        TotalInactiveAddresses++;
                    }
                }
            }
            catch (Exception)
            {
                TotalInactiveAddresses++;
            }
            finally
            {
                TotalAddressCount++;
            }
        }

        private async Task<IPScannerData> NewIPScannerDataAsync(string ipAddress, string macAddress)
        {
            var activeIP = new IPScannerData();

            activeIP.IPAddress = ipAddress;
            activeIP.MACAddress = macAddress.ToUpper();
            activeIP.Name = await _dnsHandler.GetDeviceNameAsync(ipAddress);

            await _manufacturerSemaphore.WaitAsync(TimeSpan.FromSeconds(1.2));
            activeIP.Manufacturer = await _macAddressHandler.GetManufacturerAsync(macAddress);
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
        #endregion Private Methods
    }
}
