using System.Collections.Generic;
using System.Threading.Tasks;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface ISubnetHandler
    {
        Task<List<IPv4Info>> GenerateListOfActiveSubnetsAsync();
        Task<(string _networkAddress, string _broadcastAddress)> CalculateNetworkAndBroadcastAddressesAsync(string ipAddress, string subnetMask);
    }
}
