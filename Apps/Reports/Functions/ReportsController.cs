using NetworkAnalyzer.Apps.Reports.Interfaces;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal delegate void UpdateAvailableSessionDataEventHandler();

    internal class ReportsController : IReportsController
    {
        public event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;

        public void SendUpdateAvailableSessionDataRequest()
        {
            UpdateAvailableSessionData?.Invoke();
        }
    }
}
