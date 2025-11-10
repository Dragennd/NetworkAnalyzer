using NetworkAnalyzer.Apps.Reports.Functions;

namespace NetworkAnalyzer.Apps.Reports.Interfaces
{
    internal interface IReportsController
    {
        event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;
        event SetAvailableSessionDataEventHandler SetUserDefinedTargetData;

        void SendUpdateAvailableSessionDataRequest();
        void SendSetUserDefinedTargetDataRequest(string data);
    }
}
