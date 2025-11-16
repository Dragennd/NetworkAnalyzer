namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IMACAddressHandler
    {
        Task<string> GetMACAddressAsync(string ipAddress);
        Task<string> GetManufacturerAsync(string macAddress);
    }
}
