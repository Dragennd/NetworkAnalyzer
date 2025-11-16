namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IRDPHandler
    {
        Task<bool> ScanRDPPortAsync(string ipAddress);
        Task StartRDPSessionAsync(string ipAddress);
    }
}
