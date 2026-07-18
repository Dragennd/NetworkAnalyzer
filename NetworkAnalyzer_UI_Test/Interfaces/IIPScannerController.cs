using NetworkAnalyzer_UI_Test.EventControllers;
using NetworkAnalyzer_UI_Test.Functions;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.Interfaces
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
