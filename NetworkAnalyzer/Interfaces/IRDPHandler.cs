using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IRDPHandler
    {
        Task<bool> ScanRDPPortAsync(string ipAddress);
        Task StartRDPSessionAsync(string ipAddress);
    }
}
