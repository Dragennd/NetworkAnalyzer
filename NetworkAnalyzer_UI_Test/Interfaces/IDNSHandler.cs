using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IDNSHandler
    {
        Task<string> GetDeviceNameAsync(string ipAddress);
        Task<string> ResolveIPAddressFromDNSAsync(string target);
    }
}
