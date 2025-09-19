using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface ISubnetHandler
    {
        Task<List<IPv4Info>> GenerateListOfActiveSubnetsAsync();
        Task<(string _networkAddress, string _broadcastAddress)> CalculateNetworkAndBroadcastAddressesAsync(string ipAddress, string subnetMask);
    }
}
