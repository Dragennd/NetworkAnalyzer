using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal class SubnetHandler : ISubnetHandler
    {
        public async Task<List<IPv4Info>> GenerateListOfActiveSubnetsAsync()
        {
            var filteredAddresses = await GetActiveNetworkInterfacesAsync();
            filteredAddresses = await filteredAddresses.RemoveDuplicateSubnetAsync();

            foreach (var address in filteredAddresses)
            {
                var calculatedAddressData = await CalculateNetworkAndBroadcastAddressesAsync(address.IPv4Address, address.SubnetMask);

                address.NetworkAddress = calculatedAddressData._networkAddress;
                address.BroadcastAddress = calculatedAddressData._broadcastAddress;
            }

            return filteredAddresses;
        }

        private async Task<List<IPv4Info>> GetActiveNetworkInterfacesAsync()
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
                    .Select(a => new IPv4Info($"{a.Address} {a.IPv4Mask}", true))
                    .ToList();

            // Add the instance of the IPv4Info list which contains the IPv4 Addresses and Subnet Masks that passed the filtering to the temp list
            return await Task.FromResult(filteredIPAddresses);
        }

        private async Task<(string _networkAddress, string _broadcastAddress)> CalculateNetworkAndBroadcastAddressesAsync(string ipAddress, string subnetMask)
        {
            // Parse IP and Subnet Mask into an IP Address
            IPAddress ip = IPAddress.Parse(ipAddress);
            IPAddress mask = IPAddress.Parse(subnetMask);

            // Convert the IP and Subnet Mask into 32-bit unsigned integers
            uint ipInt = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
            uint maskInt = BitConverter.ToUInt32(mask.GetAddressBytes().Reverse().ToArray(), 0);

            // Calculate the Network and Broadcast addresses by performing bitwise operations
            uint network = ipInt & maskInt;
            uint broadcast = network | ~maskInt;

            // Convert the Network and Broadcast addresses back to an IP Address
            IPAddress networkAddress = new(BitConverter.GetBytes(network).Reverse().ToArray());
            IPAddress broadcastAddress = new(BitConverter.GetBytes(broadcast).Reverse().ToArray());

            // Compile the addresses and return them as strings
            return await Task.FromResult((networkAddress.ToString(), broadcastAddress.ToString()));
        }
    }
}
