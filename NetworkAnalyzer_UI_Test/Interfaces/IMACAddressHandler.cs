using System.Threading.Tasks;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IMACAddressHandler
    {
        Task<string> GetMACAddressAsync(string ipAddress);
        Task<string> GetManufacturerAsync(string macAddress);
    }
}
