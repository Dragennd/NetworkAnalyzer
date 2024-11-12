using System.Net.NetworkInformation;
using System.Windows;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.GlobalClasses;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SubnetMaskHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.MACAddressHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.RDPHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SMBHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SSHHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal delegate void IPScannerResultsEventHandler(IPScannerData data);

    internal class IPScannerManager
    {
        public event IPScannerResultsEventHandler IPScannerResults;

        private SemaphoreSlim _semaphore = new(1, 1);

        // Process all IP Scanner data objects and add them to the ScanData ObservableCollection in the View Model
        public async Task ProcessIPScannerDataAsync()
        {
            var dbHandler = new DatabaseHandler();
            var tasks = new List<Task>();
            var processingSemaphore = new SemaphoreSlim(1, 1);
            var log = new LogHandler();

            // Create a list of scannable IP Addresses
            foreach (var address in await GetScanList())
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var ping = new Ping())
                        {
                            var pingResult = await ping.SendPingAsync(address, 1000);

                            if (pingResult.Status == IPStatus.Success)
                            {
                                var mac = await GetMACAddress(pingResult.Address.ToString());
                                var results = await NewIPScannerDataAsync(pingResult.Address.ToString(), mac);

                                await dbHandler.NewIPScannerReportEntryAsync(results);

                                await Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    IPScannerResults.Invoke(results);
                                });

                                await _semaphore.WaitAsync();
                                TotalActiveIPAddresses++;
                                _semaphore.Release();
                            }
                            else
                            {
                                await _semaphore.WaitAsync();
                                TotalInactiveIPAddresses++;
                                _semaphore.Release();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        await _semaphore.WaitAsync();
                        TotalInactiveIPAddresses++;
                        _semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            processingSemaphore.Dispose();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        #region Private Methods
        // Generate a new IPScanner object
        private async Task<IPScannerData> NewIPScannerDataAsync(string ipAddress, string macAddress)
        {
            var activeIP = new IPScannerData();

            activeIP.IPAddress = ipAddress;
            activeIP.MACAddress = macAddress.ToUpper();
            activeIP.Name = await GetDeviceNameAsync(ipAddress);
            activeIP.Manufacturer = await SendAPIRequestAsync(macAddress);
            activeIP.RDPEnabled = await ScanRDPPortAsync(ipAddress);
            activeIP.SMBEnabled = await ScanSMBPortAsync(ipAddress);
            activeIP.SSHEnabled = await ScanSSHPortAsync(ipAddress);

            return activeIP;
        }

        private async Task<List<string>> GetScanList()
        {
            var scanList = new List<string>();

            foreach (var subnet in ActiveSubnets)
            {
                foreach (var address in await GenerateScanListAsync(subnet))
                {
                    scanList.Add(address);
                }
            }

            TotalSizeOfSubnetToScan = scanList.Count;

            return await Task.FromResult(scanList);
        }
        #endregion
    }
}
