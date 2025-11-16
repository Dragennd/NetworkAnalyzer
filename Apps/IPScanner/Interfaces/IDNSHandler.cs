namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IDNSHandler
    {
        Task<string> GetDeviceNameAsync(string ipAddress);
        Task<string> ResolveIPAddressFromDNSAsync(string target);
    }
}
