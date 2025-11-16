using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.IPScanner.Functions
{
    internal delegate void IPScannerResultsEventHandler(IPScannerData data);

    internal delegate void IPScannerIntEventHandler(int num);

    internal delegate void IPScannerStringEventHandler(string str);

    internal class IPScannerController : IIPScannerController
    {
        public event IPScannerResultsEventHandler AddScanResults;

        public event IPScannerStringEventHandler UpdateSubnetsToScan;

        public event IPScannerStringEventHandler UpdateScanStatus;

        public event IPScannerIntEventHandler UpdateTotalAddressCount;

        public event IPScannerIntEventHandler UpdateTotalActiveAddresses;

        public event IPScannerIntEventHandler UpdateTotalInactiveAddresses;

        public void SendAddScanResultsRequest(IPScannerData data)
        {
            AddScanResults?.Invoke(data);
        }

        public void SendUpdateSubnetsToScanRequest(string str)
        {
            UpdateSubnetsToScan?.Invoke(str);
        }

        public void SendUpdateScanStatusRequest(string str)
        {
            UpdateScanStatus?.Invoke(str);
        }

        public void SendUpdateTotalAddressCountRequest(int num)
        {
            UpdateTotalAddressCount?.Invoke(num);
        }

        public void SendUpdateTotalActiveAddressesRequest(int num)
        {
            UpdateTotalActiveAddresses?.Invoke(num);
        }

        public void SendUpdateTotalInactiveAddressesRequest(int num)
        {
            UpdateTotalInactiveAddresses?.Invoke(num);
        }
    }
}
