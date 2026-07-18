using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IDNSHandler
    {
        Task<string> GetDeviceNameAsync(string ipAddress);
        Task<string> ResolveIPAddressFromDNSAsync(string target);
    }
}
