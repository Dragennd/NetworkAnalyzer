using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Interfaces
{
    internal interface ISubnetHandler
    {
        Task<List<IPv4Info>> GenerateListOfActiveSubnetsAsync();
        Task<(string _networkAddress, string _broadcastAddress)> CalculateNetworkAndBroadcastAddressesAsync(string ipAddress, string subnetMask);
    }
}
