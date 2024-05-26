using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public class SubnetMaskHandler
    {
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

            // Return the instance of the IPv4Info list which contains the IPv4 Addresses and Subnet Masks that passed the filtering
            return await RemoveDuplicateSubnetAsync(addresses);
        }

        public static async Task<List<IPv4Info>> GetIPBoundsAsync(List<IPv4Info> ipInfoCollection)
        {
            foreach (var entry in ipInfoCollection)
            {
                // Calculate the upper and lower bounds used to generate the IP Addresses for scanning
                entry.IPBounds = await CalculateIPBoundsAsync(entry);
            }

            // Return the instance of the IPv4Info list which contains the IP Bounds
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
                            // Add a task to generate the next IP Address in the list
                            tasks.Add(GenerateIPAddressAsync(info.IPv4Address, h, i, j, k));
                        }
                    }
                }
            }

            // When the IP Address generation is complete, return all IP Addresses as a string array
            return await Task.WhenAll(tasks);
        }

        public static async Task<List<IPv4Info>> ValidateUserInputAsync(string userInput)
        {
            List<IPv4Info> validatedUserInput = new();
            const string ipWithCIDR = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\/(?:3[0-2]|[1-2]?[0-9])\b";
            const string ipWithSubnetMask = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\.(?:255|254|252|248|240|224|192|128|0)\b";
            const string ipRange = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\s*-\s*(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9]?[0-9])\b";

            // If user input is an IP Address followed by cidr notation (e.g. 172.30.1.1/24)
            if (Regex.IsMatch(userInput, ipWithCIDR))
            {
                validatedUserInput.Add(await ParseIPWithCIDRAsync(userInput));
            }
            // If user input is an IP Address followed by the full subnet mask (e.g. 172.30.1.1 255.255.255.0)
            else if (Regex.IsMatch(userInput, ipWithSubnetMask))
            {
                validatedUserInput.Add(await ParseIPWithSubnetMaskAsync(userInput));
            }
            // If user input is two IP Addresses separated by a hyphen (e.g. 172.30.1.1 - 172.30.1.50)
            else if (Regex.IsMatch(userInput, ipRange))
            {
                validatedUserInput.Add(await ParseIPRangeAsync(userInput));
            }
            // If user input doesn't match the proper formatting, throw an error
            else
            {
                throw new ExceptionHandler(ResponseCode.InvalidInputException);
            }

            return await Task.FromResult(validatedUserInput);
        }

        private static async Task<IPv4Info> ParseIPWithCIDRAsync(string userInput)
        {
            IPv4Info info = new();

            int hostBits = int.Parse(userInput.Split("/")[1]);
            int[] maskParts = new int[4];

            // Loop through the octets of the Subnet Mask
            // and assign a mask to that position based upon the provided CIDR Notation
            for (int i = 0; i < maskParts.Length; i++)
            {
                if (hostBits >= 8)
                {
                    maskParts[i] = 255;
                    hostBits -= 8;
                }
                else if (hostBits > 0)
                {
                    maskParts[i] = 255 - ((int)Math.Pow(2, 8 - hostBits) - 1);
                    hostBits = 0;
                }
                else
                {
                    maskParts[i] = 0;
                }
            }

            info.IPv4Address = userInput.Split("/")[0];
            info.SubnetMask = string.Join(".", maskParts);

            return await Task.FromResult(info);
        }

        private static async Task<IPv4Info> ParseIPWithSubnetMaskAsync(string userInput)
        {
            IPv4Info info = new();

            info.IPv4Address = userInput.Split(' ')[0];
            info.SubnetMask = userInput.Split(' ')[1];

            return await Task.FromResult(info);
        }

        private static async Task<IPv4Info> ParseIPRangeAsync(string userInput)
        {
            IPv4Info info = new();
            string ip1 = userInput.Split("-")[0].Trim();
            string ip2 = userInput.Split("-")[1].Trim();
            bool validInput = true;

            // Check each octet from the IP Range to see if the left IP is greater than the right IP
            for (int i = 3; i >= 1; i--)
            {
                if (int.Parse(ip1.Split(".")[i]) > int.Parse(ip2.Split(".")[i]) &&
                  ((int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1])) ||
                   (int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]))))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) == int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) == int.Parse(ip2.Split(".")[i - 1]) &&
                         validInput == false)
                {
                    validInput = false;
                }
                else if (int.Parse(ip1.Split(".")[i]) < int.Parse(ip2.Split(".")[i]) &&
                         int.Parse(ip1.Split(".")[i - 1]) > int.Parse(ip2.Split(".")[i - 1]))
                {
                    validInput = false;
                }
                else
                {
                    validInput = true;
                }
            }

            if (validInput)
            {
                info.IPv4Address = ip1;
                info.SubnetMask = ip2;
            }
            else
            {
                throw new ExceptionHandler(ResponseCode.BadRangeException);
            }

            return await Task.FromResult(info);
        }

        private static async Task<List<int>> CalculateIPBoundsAsync(IPv4Info info)
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

        private static async Task<List<IPv4Info>> RemoveDuplicateSubnetAsync(List<IPv4Info> addresses)
        {
            var firstOctet = new List<string>();
            var secondOctet = new List<string>();
            var thirdOctet = new List<string>();

            // Make sure the listed IP Addresses aren't empty by checking each octet
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

            // Return the instance of the IPv4Info list which has been filtered of any duplicate IP Addresses/Subnets
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

            // Return the re-combined IP Address as a string
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
