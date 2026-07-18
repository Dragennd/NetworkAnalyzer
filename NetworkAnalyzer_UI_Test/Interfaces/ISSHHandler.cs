using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface ISSHHandler
    {
        Task<bool> ScanSSHPortAsync(string ipAddress);
        Task StartSSHSessionAsync(string ipAddress);
    }
}
