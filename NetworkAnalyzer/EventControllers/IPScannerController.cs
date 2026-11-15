using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.EventControllers;

internal delegate void IPScannerResultsEventHandler(IPScannerData data);
internal delegate void IPScannerIntEventHandler(int num);
internal delegate void IPScannerStringEventHandler(string str);
internal delegate void IPScannerBoolEventHandler(bool b);

internal class IPScannerController
{
    public event IPScannerResultsEventHandler AddScanResults;
    public event IPScannerStringEventHandler UpdateSubnetsToScan;
    public event IPScannerStringEventHandler UpdateScanStatus;
    public event IPScannerIntEventHandler UpdateTotalAddressCount;
    public event IPScannerIntEventHandler UpdateTotalActiveAddresses;
    public event IPScannerIntEventHandler UpdateTotalInactiveAddresses;
    public event IPScannerBoolEventHandler SetEmergencyStopStatus;

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

    public void SendSetEmergencyStopStatusRequest(bool b)
    {
        SetEmergencyStopStatus?.Invoke(b);
    }
}