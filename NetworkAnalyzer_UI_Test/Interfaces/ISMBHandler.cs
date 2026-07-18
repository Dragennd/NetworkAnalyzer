using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface ISMBHandler
    {
        Task<bool> ScanSMBPortAsync(string ipAddress);
        Task StartSMBSessionAsync(string ipAddress);
    }
}
