using NetworkAnalyzer.Apps.GlobalClasses;
using System.Net.NetworkInformation;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.IPScanner
{
    public class IPScannerFunction
    {
        // Scan network using IP Bounds generated in the SubnetMaskHandler Class
        public static async Task GetActiveIPAddressesAsync()
        {
            List<Task<PingReply>> ipTasks = new();

            // Generate the upper and lower bounds for the provided IP Addresses from the network interface cards on the local computer
            var info = await SubnetMaskHandler.GetIPBoundsAsync(await SubnetMaskHandler.GetActiveNetworkInterfacesAsync());

            foreach (var item in info)
            {
                // Create a list of scannable IP Addresses
                var addresses = await SubnetMaskHandler.GenerateScanListAsync(item);

                foreach (var address in addresses)
                {
                    // Loop through the provided list and create a list of tasks to ping all of the provided IP Addresses
                    ipTasks.Add(new Ping().SendPingAsync(address, 1000));
                }
            }

            try
            {
                foreach (var task in await Task.WhenAll(ipTasks))
                {
                    if (task.Status == IPStatus.Success)
                    {
                        lock (ScanResultsLock)
                        {
                            // Lock the ScanResults ConcurrentBag and add the pinged IP Address if it returned an IPStatus of Success
                            ScanResults.Add(new IPScanData() { IPAddress = task.Address.ToString() });
                        }
                    }
                }
            }
            catch (PingException)
            {
                // Do nothing if the ping fails
            }
        }

        // Send an ARP request to every IP Address that was returned with the GetActiveIPAddresses method
        public static async Task GetActiveMACAddressesAsync()
        {
            List<Task> tasks = new();

            foreach (var item in ScanResults)
            {
                Task task = Task.Run(async () =>
                {
                    var mac = await MACAddressHandler.GetMACAddress(item.IPAddress);

                    lock (ScanResultsLock)
                    {
                        // Lock the ScanResults ConcurrentBag and add the MAC Address of the target IP Address
                        item.MACAddress = mac;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        // Send an API call to https://api.maclookup.app/v2/macs/ to request the manufacturer of the MAC Address
        public static async Task GetMACAddressInfoAsync()
        {
            foreach (var item in ScanResults)
            {
                // Send a REST API call to request the Manufacturer associated with the MAC Address
                var manufacturer = await MACAddressHandler.SendAPIRequestAsync(item.MACAddress);

                lock (ScanResultsLock)
                {
                    // Lock the ScanResults ConcurrentBag and add the Manufacturer from the API response
                    item.Manufacturer = manufacturer;
                }
            }
        }

        // Request the DNS name for a device by getting the Host Entry
        public static async Task GetDNSHostNameAsync()
        {
            List<Task> tasks = new();

            foreach (var item in ScanResults)
            {
                Task task = Task.Run(async () =>
                {
                    // Attempt to resolve the DNS Host Entry for the target IP Address
                    var dns = await DNSHandler.GetDeviceNameAsync(item.IPAddress);

                    lock (ScanResultsLock)
                    {
                        // Lock the ScanResults ConcurrentBag and add the resolved DNS host name for the target IP Address
                        item.Name = dns;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        // Check to see if SMB is available on a device
        public static async Task GetSMBPortAvailabilityAsync()
        {
            List<Task> tasks = new();

            foreach (var item in ScanResults)
            {
                Task task = Task.Run(async () =>
                {
                    // Check if SMB is enabled on the target IP Address
                    var smb = await SMBHandler.ScanSMBPortAsync(item.IPAddress);

                    lock (ScanResultsLock)
                    {
                        // Lock the ScanResults ConcurrentBag and set the SMBEnabled boolean accordingly
                        item.SMBEnabled = smb;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        // Check to see if SSH is available on a device
        public static async Task GetSSHPortAvailabilityAsync()
        {
            List<Task> tasks = new();

            foreach (var item in ScanResults)
            {
                Task task = Task.Run(async () =>
                {
                    // Check if SSH is enabled on the target IP Address
                    var ssh = await SSHHandler.ScanSSHPortAsync(item.IPAddress);

                    lock (ScanResultsLock)
                    {
                        // Lock the ScanResults ConcurrentBag and set the SSHEnabled boolean accordingly
                        item.SSHEnabled = ssh;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        // Check to see if RDP is available on a device
        public static async Task GetRDPPortAvailabilityAsync()
        {
            List<Task> tasks = new();

            foreach (var item in ScanResults)
            {
                Task task = Task.Run(async () =>
                {
                    // Check if RDP is enabled on the target IP Address
                    var rdp = await RDPHandler.ScanRDPPortAsync(item.IPAddress);

                    lock (ScanResultsLock)
                    {
                        // Lock the ScanResults ConcurrentBag and set the RDPEnabled boolean accordingly
                        item.RDPEnabled = rdp;
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}
