using NetworkAnalyzer.Apps.GlobalClasses;

namespace NetworkAnalyzer.Apps.IPScanner
{
    public class IPScannerFunction
    {
        // Scan network using IP Bounds generated in the SubnetMaskHandler Class
        public static async Task GetActiveIPAddressesAsync()
        {
            SubnetMaskHandler.GenerateListOfSubnetsFromLocalIPs();

            // Execute the Network Scan for each confirmed subnet
            foreach (var item in SubnetMaskHandler.CurrentActiveSubnetInfo)
            {
                await Task.Run(() => SubnetMaskHandler.GenerateScanList(item.IPv4Address, item.IPBounds.ToList()));
            }

            // Once the IP Addresses have been generated, execute the Ping Test on each of them to check their availability
            Parallel.ForEach (SubnetMaskHandler.scanAddresses, async address =>
            {
                string ip = await SubnetMaskHandler.ExecutePingTest(address);
                if (!string.IsNullOrWhiteSpace(ip))
                {
                    DataStore.ScanResults.Add(new DataStore.IPScanData() { IPAddress = ip });
                }
            });
        }

        // Send an ARP request to every IP Address that was returned with the GetActiveIPAddresses method
        public static async Task GetActiveMACAddressesAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.MACAddress = await MACAddressHandler.GetMACAddress(item.IPAddress);
            }
        }

        // Send an API call to https://api.maclookup.app/v2/macs/ to request the manufacturer of the MAC Address
        public static async Task GetMACAddressInfoAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.Manufacturer = await MACAddressHandler.SendAPIRequestAsync(item.MACAddress);
            }
        }

        // Request the DNS name for a device by getting the Host Entry
        public static async Task GetDNSHostNameAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.Name = await DNSHandler.GetDeviceNameAsync(item.IPAddress);
            }
        }

        // Check to see if SMB is available on a device
        public static async Task GetSMBPortAvailabilityAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.SMBEnabled = await SMBHandler.ScanSMBPortAsync(item.IPAddress);
            }
        }

        // Check to see if SSH is available on a device
        public static async Task GetSSHPortAvailabilityAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.SSHEnabled = await SSHHandler.ScanSSHPortAsync(item.IPAddress);
            }
        }

        // Check to see if RDP is available on a device
        public static async Task GetRDPPortAvailabilityAsync()
        {
            foreach (var item in DataStore.ScanResults)
            {
                item.RDPEnabled = await RDPHandler.ScanRDPPortAsync(item.IPAddress);
            }
        }
    }
}
