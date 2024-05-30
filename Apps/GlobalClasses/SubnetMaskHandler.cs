using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class SubnetMaskHandler
    {
        private List<IPv4Info> tempInfo = new();

        public SubnetMaskHandler()
        {

        }

        public SubnetMaskHandler(IPv4Info info)
        {
            tempInfo.Add(info);
        }

        public async Task GetActiveNetworkInterfacesAsync()
        {
            // Get all NICs from the computer performing the scan
            var interfaceAddresses = await Task.Run(() => NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses));

            // Filter out the IPv6, APIPA and Link Local network interfaces
            var temp = interfaceAddresses
                    .Where(a =>
                           a.Address.ToString().Split(".").Length == 4 &&
                         !(a.Address.ToString().Split(".")[0] == "127" ||
                          (a.Address.ToString().Split(".")[0] == "169" && a.Address.ToString().Split(".")[1] == "254") ||
                           a.Address.ToString().Contains(':')))
                    .Select(a => (new IPv4Info() { IPv4Address = a.Address.ToString(), SubnetMask = a.IPv4Mask.ToString() }))
                    .ToList();

            // Add the instance of the IPv4Info list which contains the IPv4 Addresses and Subnet Masks that passed the filtering to the temp list
            tempInfo =  await RemoveDuplicateSubnetAsync(temp);
        }

        public async Task<List<IPv4Info>> GetIPBoundsAsync()
        {
            foreach (var entry in tempInfo)
            {
                // Calculate the upper and lower bounds used to generate the IP Addresses for scanning
                entry.IPBounds = await CalculateIPBoundsAsync(entry);
            }

            // Return the instance of the IPv4Info list which contains the IP Bounds
            return tempInfo;
        }

        public async Task<string[]> GenerateScanListAsync(IPv4Info info)
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
                            // Add a task to generate the next IP Address in the list
                            tasks.Add(GenerateIPAddressAsync(info.IPv4Address, h, i, j, k));
                        }
                    }
                }
            }

            // When the IP Address generation is complete, return all IP Addresses as a string array
            return await Task.WhenAll(tasks);
        }

        private async Task<List<int>> CalculateIPBoundsAsync(IPv4Info info)
        {
            string[] subnetOctet = info.SubnetMask.Split(".");
            string[] ipOctet = info.IPv4Address.Split(".");

            int subnetSize = 0;
            int lowerBound = 0;
            int upperBound = 256;

            // Hit this if only if the provided user input is a range of IP Addresses and not a specified subnet
            if ((subnetOctet[0] == "192" && subnetOctet[1] == "168") || subnetOctet[0] == ipOctet[0])
            {
                foreach (var item in ipOctet.Zip(subnetOctet, (a, b) => new { ip = a, sub = b }))
                {
                    if (item.sub != item.ip)
                    {
                        info.IPBounds.Add(int.Parse(item.sub));
                        info.IPBounds.Add(int.Parse(item.ip));
                    }
                }
            }
            else
            {
                // Create a two-dimensional array containing the individual IP Address and Subnet Mask octets and loop through them
                foreach (var item in ipOctet.Zip(subnetOctet, (a, b) => new { ip = a, sub = b }))
                {
                    subnetSize = upperBound - int.Parse(item.sub);
                    upperBound = lowerBound + subnetSize - 1;

                    // Loop through the provided two-dimensional array until the correct upper and lower bounds are located for each octet
                    while (!(int.Parse(item.sub) != 0) && subnetSize > 2)
                    {
                        // If the correct upper and lower bounds are found, add them to the IPv4Info list instance and end the loop
                        if (int.Parse(item.ip) <= upperBound && int.Parse(item.ip) >= lowerBound)
                        {
                            info.IPBounds.Add(upperBound);
                            info.IPBounds.Add(lowerBound);
                            break;
                        }
                        else
                        {
                            // If the upper and lower bounds do not match, increment them by the size of the subnet and loop again
                            upperBound += subnetSize;
                            lowerBound += subnetSize;
                        }
                    }

                    // Reset the upper and lower bound variables for the next octet in the list
                    upperBound = 256;
                    lowerBound = 0;
                }
            }

            // Add placeholder ints to the IPv4Info list instance for the ipBounds corresponding with the octet the loop is on
            while (info.IPBounds.Count() < 8)
            {
                info.IPBounds.Add(300);
            }

            // When the upper and lower bounds have been generated, return them as a list of type int
            return await Task.FromResult(info.IPBounds);
        }

        private async Task<List<IPv4Info>> RemoveDuplicateSubnetAsync(List<IPv4Info> addresses)
        {
            var firstOctet = new List<string>();
            var secondOctet = new List<string>();
            var thirdOctet = new List<string>();

            // Make sure the listed IP Addresses aren't empty by checking each octet
            foreach (var item in addresses)
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
                    addresses.RemoveAt(0);
                }
            }

            // Return the instance of the IPv4Info list which has been filtered of any duplicate IP Addresses/Subnets
            return await Task.FromResult(addresses);
        }

        private async Task<string> GenerateIPAddressAsync(string ipAddress, int replacementOctet1, int replacementOctet2, int replacementOctet3, int replacementOctet4)
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

            // Return the re-combined IP Address as a string
            return await Task.FromResult(string.Join(".", ipArray));
        }
    }
}
