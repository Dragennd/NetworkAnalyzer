using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IHomeController
    {
        event HomeChangelogUpdateEventHandler UpdateChangelog;
        event HomeNetworkStatusUpdateEventHandler UpdateIPv4;
        event HomeNetworkStatusUpdateEventHandler UpdateIPv6;
        event HomeNetworkStatusUpdateEventHandler UpdateDNS;

        void SendUpdateChangelogRequest();
        void SendUpdateIPv4Request(NetworkStatusInfo networkStatusInfo);
        void SendUpdateIPv6Request(NetworkStatusInfo networkStatusInfo);
        void SendUpdateDNSRequest(NetworkStatusInfo networkStatusInfo);
    }
}
