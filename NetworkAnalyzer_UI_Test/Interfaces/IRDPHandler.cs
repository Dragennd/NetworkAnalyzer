using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IRDPHandler
    {
        Task<bool> ScanRDPPortAsync(string ipAddress);
        Task StartRDPSessionAsync(string ipAddress);
    }
}
