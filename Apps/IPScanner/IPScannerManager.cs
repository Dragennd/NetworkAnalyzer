using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SubnetMaskHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.MACAddressHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.RDPHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SMBHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.SSHHandler;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal class IPScannerManager
    {
        private SemaphoreSlim _semaphore = new(1,1);

        // Add the IPScanner object to the 
        public async Task AddIPScannerDataAsync()
        {
            var tasks = new List<Task>();

            // Generate the upper and lower bounds for the provided IP Addresses from the network interface cards on the local computer
            foreach (var subnet in ActiveSubnets)
            {
                // Create a list of scannable IP Addresses
                foreach (var address in await GenerateScanListAsync(subnet))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var pingResult = await new Ping().SendPingAsync(address, 1000);
                            var mac = await GetMACAddress(pingResult.Address.ToString());

                            if (pingResult.Status == IPStatus.Success)
                            {
                                ScanResults.Add(await NewIPScannerDataAsync(pingResult.Address.ToString(), mac));

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
                        catch (PingException)
                        {
                            await _semaphore.WaitAsync();
                            TotalInactiveIPAddresses++;
                            _semaphore.Release();
                        }
                        catch (ArgumentNullException)
                        {
                            await _semaphore.WaitAsync();
                            TotalInactiveIPAddresses++;
                            _semaphore.Release();
                        }

                        // Increment the tracker for how many addresses are being pinged
                        await _semaphore.WaitAsync();
                        TotalSizeOfSubnetToScan++;
                        _semaphore.Release();
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }

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
    }
}
