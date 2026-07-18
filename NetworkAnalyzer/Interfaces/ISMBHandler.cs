using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface ISMBHandler
    {
        Task<bool> ScanSMBPortAsync(string ipAddress);
        Task StartSMBSessionAsync(string ipAddress);
    }
}
