using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface ISSHHandler
    {
        Task<bool> ScanSSHPortAsync(string ipAddress);
        Task StartSSHSessionAsync(string ipAddress);
    }
}
