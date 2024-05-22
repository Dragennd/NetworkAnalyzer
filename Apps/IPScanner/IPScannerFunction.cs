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

            var info = await SubnetMaskHandler.GetIPBoundsAsync(await SubnetMaskHandler.GetActiveNetworkInterfacesAsync());

            foreach (var item in info)
            {
                var addresses = await SubnetMaskHandler.GenerateScanListAsync(item);

                foreach (var address in addresses)
                {
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
            foreach (var item in ScanResults)
            {
                var mac = await MACAddressHandler.GetMACAddress(item.IPAddress);

                lock (ScanResultsLock)
                {
                    item.MACAddress = mac;
                }
            }
        }

        // Send an API call to https://api.maclookup.app/v2/macs/ to request the manufacturer of the MAC Address
        public static async Task GetMACAddressInfoAsync()
        {
            foreach (var item in ScanResults)
            {
                var manufacturer = await MACAddressHandler.SendAPIRequestAsync(item.MACAddress);

                lock (ScanResultsLock)
                {
                    item.Manufacturer = manufacturer;
                }
            }
        }

        // Request the DNS name for a device by getting the Host Entry
        public static async Task GetDNSHostNameAsync()
        {
            foreach (var item in ScanResults)
            {
                var dns = await DNSHandler.GetDeviceNameAsync(item.IPAddress);

                lock (ScanResultsLock)
                {
                    item.Name = dns;
                }
            }
        }

        // Check to see if SMB is available on a device
        public static async Task GetSMBPortAvailabilityAsync()
        {
            foreach (var item in ScanResults)
            {
                var smb = await SMBHandler.ScanSMBPortAsync(item.IPAddress);

                lock (ScanResultsLock)
                {
                    item.SMBEnabled = smb;
                }
            }
        }

        // Check to see if SSH is available on a device
        public static async Task GetSSHPortAvailabilityAsync()
        {
            foreach (var item in ScanResults)
            {
                var ssh = await SSHHandler.ScanSSHPortAsync(item.IPAddress);

                lock (ScanResultsLock)
                {
                    item.SSHEnabled = ssh;
                }
            }
        }

        // Check to see if RDP is available on a device
        public static async Task GetRDPPortAvailabilityAsync()
        {
            foreach (var item in ScanResults)
            {
                var rdp = await RDPHandler.ScanRDPPortAsync(item.IPAddress);

                lock (ScanResultsLock)
                {
                    item.RDPEnabled = rdp;
                }
            }
        }
    }
}
