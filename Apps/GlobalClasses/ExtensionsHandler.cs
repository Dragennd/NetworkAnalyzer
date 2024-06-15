using NetworkAnalyzer.Apps.Models;
using System.Text.RegularExpressions;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public static class ExtensionsHandler
    {
        public static async Task<List<IPv4Info>> RemoveDuplicateSubnetAsync(this List<IPv4Info> filteredAndUniqueIPAddresses)
        {
            return await Task.Run(() =>
            {
                var firstOctet = new List<string>();
                var secondOctet = new List<string>();
                var thirdOctet = new List<string>();

                // Make sure the listed IP Addresses aren't empty by checking each octet
                foreach (var item in filteredAndUniqueIPAddresses)
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
                        filteredAndUniqueIPAddresses.RemoveAt(0);
                    }
                }

                // Return the instance of the IPv4Info list which has been filtered of any duplicate IP Addresses/Subnets
                return filteredAndUniqueIPAddresses;
            });
        }

        public static string FormatAsMacAddress(this string macAddress)
        {
            var regex = "^([a-fA-F0-9]{2}){6}$";
            return string.Join(":", Regex.Match(macAddress, regex).Groups[1].Captures.Select(x => x.Value));
        }
    }
}
