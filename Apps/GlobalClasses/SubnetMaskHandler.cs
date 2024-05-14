using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class SubnetMaskHandler
    {
        public static List<IPv4Info> CurrentActiveSubnetInfo = new();
        public static List<string> scanAddresses = new();

        public static void GenerateScanList(string ipAddress, List<int> ipBounds)
        {
            // Add placeholder ints to the ipBounds List for any IP Address octets that won't need to be replaced
            while (ipBounds.Count() <= 8)
            {
                ipBounds.Add(900);
            }
            
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
                            scanAddresses.Add(GenerateIPToScan(ipAddress, h, i, j, k));
                        }
                    }
                }
            }
        }

        public static void GenerateListOfSubnetsFromLocalIPs()
        {
            // Gather Unicast Address information from all NICs on the computer the scan is running on
            var ipV4Addresses = NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses);

            foreach (var item in ipV4Addresses)
            {
                // All NICs that return an IP Address, IPv4 or IPv6, add the info to the List below
                CurrentActiveSubnetInfo.Add(new IPv4Info()
                {
                    IPv4Address = item.Address.ToString(),
                    SubnetMask = item.IPv4Mask.ToString()
                });
            }

            // Remove all APIPA, localhost and IPv6 addresses from the List
            CleanUpListOfSubnets();

            // Remove duplicate Subnets from the List
            if (CurrentActiveSubnetInfo.Count > 1)
            {
                RemoveDuplicateSubnets();
            }
            
            // Generate the final list of IP Bounds for when the IP Addresses get generated for the scan
            CalculateIPBounds();
        }

        private static void CalculateIPBounds()
        {
            int subnetSize = 0;
            int lowerBound = 0;
            int upperBound = 256;

            foreach (var entry in CurrentActiveSubnetInfo)
            {
                string[] subnetOctet = entry.SubnetMask.Split(".");
                string[] ipOctet = entry.IPv4Address.Split(".");

                // Loop to determine the upper and lower bounds for each IP Address octet
                foreach (var item in subnetOctet.Zip(ipOctet, (a, b) => new { sub = a, ip = b }))
                {
                    subnetSize = upperBound - int.Parse(item.sub);
                    upperBound = lowerBound + subnetSize - 1;

                    while (!(int.Parse(item.sub) != 0) && subnetSize > 2)
                    {
                        
                        // When the bounds are identified, add them to the List to be used for the scan
                        if (int.Parse(item.ip) <= upperBound && int.Parse(item.ip) >= lowerBound)
                        {
                            entry.IPBounds.Add(upperBound);
                            entry.IPBounds.Add(lowerBound);
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
            }
        }

        private static void CleanUpListOfSubnets()
        {
            List<IPv4Info> temp = new();

            // Sort out all IP Addresses found and add only IP Addresses which are link-local addresses or IPv6 to the List
            foreach (var item in CurrentActiveSubnetInfo)
            {
                if ((item.IPv4Address.Split(".").Length == 4 
                && (item.IPv4Address.Split(".")[0] == "127"
                || (item.IPv4Address.Split(".")[0] == "169" && item.IPv4Address.Split(".")[1] == "254")))
                || item.IPv4Address.Contains(':'))
                {
                    temp.Add(item);
                }
            }

            // Remove any IP Addresses found in the above List from the main List
            foreach (var item in temp)
            {
                if (CurrentActiveSubnetInfo.Contains(item))
                {
                    CurrentActiveSubnetInfo.Remove(item);
                }
            }

            temp.Clear();
        }

        private static void RemoveDuplicateSubnets()
        {
            var firstOctet = new List<string>();
            var secondOctet = new List<string>();
            var thirdOctet = new List<string>();

            // Make sure the listed IP Addresses aren't empty
            // by checking each octet
            foreach (var item in CurrentActiveSubnetInfo)
            {
                if (!string.IsNullOrWhiteSpace(item.IPv4Address))
                {
                    firstOctet.Add(item.IPv4Address.Split(".")[0]);
                    secondOctet.Add(item.IPv4Address.Split(".")[1]);
                    thirdOctet.Add(item.IPv4Address.Split(".")[2]);
                }
            }

            // If the listed IP Addresses match the same first three octets as another in the list, remove it
            for (int i = 1; i < firstOctet.Count; i++)
            {
                if (firstOctet[i] == firstOctet[i - 1] && secondOctet[i] == secondOctet[i - 1] && thirdOctet[i] == thirdOctet[i - 1])
                {
                    CurrentActiveSubnetInfo.RemoveAt(0);
                }
            }
        }

        // Method to generate the IP Address for scanning
        private static string GenerateIPToScan(string ipAddress, int replacementOctet1, int replacementOctet2, int replacementOctet3, int replacementOctet4)
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
            return string.Join(".", ipArray);
        }

        public static string ExecutePingTest(string ipAddress)
        {
            // Perform the Ping Test and store the results
            var pingResults = NetworkPing.PingTest(ipAddress);

            // If the Ping Test returns a successful result, add the IP Address to the ScanResults List
            if (pingResults.Status == IPStatus.Success)
            {
                return ipAddress;
            }
            else
            {
                return string.Empty;
            }
        }

        public class IPv4Info
        {
            public string? IPv4Address { get; set; }
            public string? SubnetMask { get; set; }
            public List<int>? IPBounds { get; set; } = new();
        }
    }
}
