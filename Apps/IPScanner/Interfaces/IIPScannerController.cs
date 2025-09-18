using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IIPScannerController
    {
        event IPScannerResultsEventHandler AddScanResults;

        void SendAddScanResultsRequest(IPScannerData data);
    }
}
