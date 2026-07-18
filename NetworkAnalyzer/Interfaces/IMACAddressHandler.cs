using System.Threading.Tasks;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IMACAddressHandler
    {
        Task<string> GetMACAddressAsync(string ipAddress);
        Task<string> GetManufacturerAsync(string macAddress);
    }
}
