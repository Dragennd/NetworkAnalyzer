namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface ISSHHandler
    {
        Task<bool> ScanSSHPortAsync(string ipAddress);
        Task StartSSHSessionAsync(string ipAddress);
    }
}
