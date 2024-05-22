using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class SubnetMaskHandler
    {
        // Get the active IP Addresses and Subnet Masks from the NICs on the computer performing the scan
        // and filter out all network interfaces which are IPv6, APIPA or LinkLocal
        public static async Task<List<IPv4Info>> GetActiveNetworkInterfacesAsync()
        {
            // Get all NICs from the computer performing the scan
            var interfaceAddresses = await Task.Run(() => NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses));

            // Filter out the IPv6, APIPA and Link Local network interfaces
            List<IPv4Info> addresses =
                interfaceAddresses
                    .Where(a =>
                           a.Address.ToString().Split(".").Length == 4 &&
                         !(a.Address.ToString().Split(".")[0] == "127" ||
                          (a.Address.ToString().Split(".")[0] == "169" && a.Address.ToString().Split(".")[1] == "254") ||
                           a.Address.ToString().Contains(':')))
                    .Select(a => (new IPv4Info() { IPv4Address = a.Address.ToString(), SubnetMask = a.IPv4Mask.ToString() }))
                    .ToList();

            // Return just the IPv4 Addresses and Subnet Masks that passed the filtering
            return await RemoveDuplicateSubnetAsync(addresses);
        }

        public static async Task<List<IPv4Info>> GetIPBoundsAsync(List<IPv4Info> ipInfoCollection)
        {
            foreach (var entry in ipInfoCollection)
            {
                entry.IPBounds = await CalculateIPBoundsAsync(entry);
            }

            return ipInfoCollection;
        }

        public static async Task<string[]> GenerateScanListAsync(IPv4Info info)
        {
            List<Task<string>> tasks = new();
            List<int> ipBounds = info.IPBounds;

            // Loops through the provided bounds for the first octet of the IP Address to be generated
            for (int h = ipBounds[7]; h <= ipBounds[6]; h++)
            {
                // Loops through the provided bounds for the second octet of the IP Address to be generated
                for (int i = ipBounds[5]; i <= ipBounds[4]; i++)
                {
                    // Loops through the provided bounds for the third octet of the IP Address to be generated
                    for (int j = ipBounds[3]; j <= ipBounds[2]; j++)
                    {
                        // Loops through the provided bounds for the fourth octet of the IP Address to be generated
                        for (int k = ipBounds[1]; k <= ipBounds[0]; k++)
                        {
                            tasks.Add(GenerateIPAddressAsync(info.IPv4Address, h, i, j, k));
                        }
                    }
                }
            }

            return await Task.WhenAll(tasks);
        }

        private static async Task<List<int>> CalculateIPBoundsAsync(IPv4Info info)
        {
            string[] subnetOctet = info.SubnetMask.Split(".");
            string[] ipOctet = info.IPv4Address.Split(".");

            int subnetSize = 0;
            int lowerBound = 0;
            int upperBound = 256;

            foreach (var item in ipOctet.Zip(subnetOctet, (a, b) => new { ip = a, sub = b }))
            {
                subnetSize = upperBound - int.Parse(item.sub);
                upperBound = lowerBound + subnetSize - 1;

                while (!(int.Parse(item.sub) != 0) && subnetSize > 2)
                {
                    if (int.Parse(item.ip) <= upperBound && int.Parse(item.ip) >= lowerBound)
                    {
                        info.IPBounds.Add(upperBound);
                        info.IPBounds.Add(lowerBound);
                        break;
                    }
                    else
                    {
                        upperBound += subnetSize;
                        lowerBound += subnetSize;
                    }
                }

                upperBound = 256;
                lowerBound = 0;
            }

            // Add placeholder ints to the ipBounds List for any IP Address octets that won't need to be replaced
            while (info.IPBounds.Count() < 8)
            {
                info.IPBounds.Add(300);
            }

            return await Task.FromResult(info.IPBounds);
        }

        private static async Task<List<IPv4Info>> RemoveDuplicateSubnetAsync(List<IPv4Info> addresses)
        {
            var firstOctet = new List<string>();
            var secondOctet = new List<string>();
            var thirdOctet = new List<string>();

            // Make sure the listed IP Addresses aren't empty
            // by checking each octet
            foreach (var item in addresses)
            {
                await Task.Run(() =>
                {
                    if (!string.IsNullOrWhiteSpace(item.IPv4Address))
                    {
                        firstOctet.Add(item.IPv4Address.Split(".")[0]);
                        secondOctet.Add(item.IPv4Address.Split(".")[1]);
                        thirdOctet.Add(item.IPv4Address.Split(".")[2]);
                    }
                });
            }

            // If the listed IP Addresses match the same first three octets as another in the list, remove it
            for (int i = 1; i < firstOctet.Count; i++)
            {
                if (firstOctet[i] == firstOctet[i - 1] && secondOctet[i] == secondOctet[i - 1] && thirdOctet[i] == thirdOctet[i - 1])
                {
                    addresses.RemoveAt(0);
                }
            }

            return addresses;
        }

        private static async Task<string> GenerateIPAddressAsync(string ipAddress, int replacementOctet1, int replacementOctet2, int replacementOctet3, int replacementOctet4)
        {
            string[] ipArray = ipAddress.Split(".").ToArray();

            // Determine whether the first octet in the IP Address needs to be replaced or if its a placeholder
            if (replacementOctet1 < 256) { ipArray[0] = replacementOctet1.ToString(); }

            // Determine whether the second octet in the IP Address needs to be replaced or if its a placeholder
            if (replacementOctet2 < 256) { ipArray[1] = replacementOctet2.ToString(); }

            // Determine whether the third octet in the IP Address needs to be replaced or if its a placeholder
            if (replacementOctet3 < 256) { ipArray[2] = replacementOctet3.ToString(); }

            // Process the replacement for the fourth octet in the IP Address
            ipArray[3] = replacementOctet4.ToString();

            // Return the re-combined IP Address
            return await Task.FromResult(string.Join(".", ipArray));
        }

        public class IPv4Info
        {
            public string? IPv4Address { get; set; }
            public string? SubnetMask { get; set; }
            public List<int>? IPBounds { get; set; } = new();
        }
    }
}
