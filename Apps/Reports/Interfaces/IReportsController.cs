using NetworkAnalyzer.Apps.Reports.Functions;

namespace NetworkAnalyzer.Apps.Reports.Interfaces
{
    internal interface IReportsController
    {
        event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;

        void SendUpdateAvailableSessionDataRequest();
    }
}
