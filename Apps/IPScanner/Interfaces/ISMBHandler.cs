namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface ISMBHandler
    {
        Task<bool> ScanSMBPortAsync(string ipAddress);
        Task StartSMBSessionAsync(string ipAddress);
    }
}
