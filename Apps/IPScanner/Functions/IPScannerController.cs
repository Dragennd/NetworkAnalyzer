using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal delegate void IPScannerResultsEventHandler(IPScannerData data);

    internal class IPScannerController : IIPScannerController
    {
        public event IPScannerResultsEventHandler AddScanResults;

        public void SendAddScanResultsRequest(IPScannerData data)
        {
            AddScanResults?.Invoke(data);
        }
    }
}
