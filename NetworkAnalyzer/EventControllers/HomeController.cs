using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.EventControllers;

internal delegate void HomeChangelogUpdateEventHandler();

internal delegate void HomeNetworkStatusUpdateEventHandler(NetworkStatusInfo networkStatusInfo);

internal class HomeController
{
    public event HomeChangelogUpdateEventHandler UpdateChangelog;
    public event HomeNetworkStatusUpdateEventHandler UpdateIPv4;
    public event HomeNetworkStatusUpdateEventHandler UpdateIPv6;
    public event HomeNetworkStatusUpdateEventHandler UpdateDNS;

    public void SendUpdateChangelogRequest()
    {
        UpdateChangelog.Invoke();
    }

    public void SendUpdateIPv4Request(NetworkStatusInfo networkStatusInfo)
    {
        UpdateIPv4.Invoke(networkStatusInfo);
    }

    public void SendUpdateIPv6Request(NetworkStatusInfo networkStatusInfo)
    {
        UpdateIPv6.Invoke(networkStatusInfo);
    }

    public void SendUpdateDNSRequest(NetworkStatusInfo networkStatusInfo)
    {
        UpdateDNS.Invoke(networkStatusInfo);
    }
}