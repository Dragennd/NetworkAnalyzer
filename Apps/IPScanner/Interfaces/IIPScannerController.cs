using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Interfaces
{
    internal interface IIPScannerController
    {
        event IPScannerResultsEventHandler AddScanResults;
        event IPScannerStringEventHandler UpdateSubnetsToScan;
        event IPScannerStringEventHandler UpdateScanStatus;
        event IPScannerIntEventHandler UpdateTotalAddressCount;
        event IPScannerIntEventHandler UpdateTotalActiveAddresses;
        event IPScannerIntEventHandler UpdateTotalInactiveAddresses;

        void SendAddScanResultsRequest(IPScannerData data);
        void SendUpdateSubnetsToScanRequest(string str);
        void SendUpdateScanStatusRequest(string str);
        void SendUpdateTotalAddressCountRequest(int num);
        void SendUpdateTotalActiveAddressesRequest(int num);
        void SendUpdateTotalInactiveAddressesRequest(int num);
    }
}
