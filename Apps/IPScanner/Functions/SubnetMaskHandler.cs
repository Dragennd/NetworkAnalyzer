using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class SubnetMaskHandler
    {
        public List<IPv4Info> tempInfo = new();
        private bool manualEnabled = false;

        public SubnetMaskHandler()
        {

        }

        public SubnetMaskHandler(IPv4Info info, bool isManualEnabled)
        {
            tempInfo.Add(info);
            manualEnabled = isManualEnabled;
        }

        public async Task<List<IPv4Info>> GetActiveNetworkInterfacesAsync()
        {
            // Get all NICs from the computer performing the scan
            var interfaceAddresses = await Task.Run(() => NetworkInterface.GetAllNetworkInterfaces().SelectMany(a => a.GetIPProperties().UnicastAddresses));

            // Filter out the IPv6, APIPA and Link Local network interfaces
            var filteredIPAddresses = interfaceAddresses
                    .Where(a =>
                           a.Address.ToString().Split(".").Length == 4 &&
                         !(a.Address.ToString().Split(".")[0] == "127" ||
                          a.Address.ToString().Split(".")[0] == "169" && a.Address.ToString().Split(".")[1] == "254" ||
                           a.Address.ToString().Contains(':')))
                    .Select(a => new IPv4Info() { IPv4Address = a.Address.ToString(), SubnetMask = a.IPv4Mask.ToString() })
                    .ToList();

            // Add the instance of the IPv4Info list which contains the IPv4 Addresses and Subnet Masks that passed the filtering to the temp list
            return await Task.FromResult(filteredIPAddresses);
        }

        public async Task<List<IPv4Info>> GetIPBoundsAsync(IPScannerStatusCode status)
        {
            foreach (var entry in tempInfo)
            {
                // Calculate the upper and lower bounds used to generate the IP Addresses for scanning
                entry.IPBounds = await CalculateIPBoundsAsync(entry, manualEnabled, status);
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

        private async Task<List<int>> CalculateIPBoundsAsync(IPv4Info info, bool manualEnabled, IPScannerStatusCode status)
        {
            List<string> subnetOctet = info.SubnetMask.Split(".").ToList();
            List<string> ipOctet = info.IPv4Address.Split(".").ToList();

            int subnetSize = 0;
            int lowerBound = 0;
            int upperBound = 256;

            // Hit this only if manual mode is enabled on the main form
            // Uses the IPv4Data datatype in a weird way - only for an IP range
            // Sets the first IP Address in the IPAddress property and the second IP Address in the SubnetMask property
            if (manualEnabled && status == IPScannerStatusCode.GoodRange)
            {
                foreach (var item in ipOctet.Zip(subnetOctet, (a, b) => new { ip = a, sub = b }))
                {
                    info.IPBounds.Add(int.Parse(item.ip));
                    info.IPBounds.Add(int.Parse(item.sub));
                }

                info.IPBounds.Reverse();
            }
            else
            {
                // Create a two-dimensional array containing the individual IP Address and Subnet Mask octets and loop through them
                foreach (var item in ipOctet.Zip(subnetOctet, (a, b) => new { ip = a, sub = b }))
                {
                    subnetSize = upperBound - int.Parse(item.sub);
                    upperBound = lowerBound + subnetSize - 1;

                    // Check if the position is any index other than the last and fill in the necessary ip bounds
                    if (subnetSize > 1 && subnetSize < 255 && subnetOctet.IndexOf(item.sub) != subnetOctet.IndexOf(subnetOctet[3]))
                    {
                        int position = subnetOctet.IndexOf(item.sub) + 1;
                        while (position < 4)
                        {
                            info.IPBounds.Add(255);
                            info.IPBounds.Add(0);
                            position++;
                        }
                    }

                    // Loop through the provided two-dimensional array until the correct upper and lower bounds are located for each octet
                    do
                    {   
                        if (upperBound == 255 && info.IPBounds.Count > 0)
                        {
                            break;
                        }

                        // If the correct upper and lower bounds are found, add them to the IPv4Info list instance and end the loop
                        if (subnetSize <= 1)
                        {
                            break;
                        }
                        else if (int.Parse(item.ip) <= upperBound && int.Parse(item.ip) >= lowerBound)
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
                    } while (upperBound <= 256);

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
