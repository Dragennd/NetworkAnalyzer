using NetworkAnalyzer.Apps.Reports.Interfaces;

namespace NetworkAnalyzer.Apps.Reports.Functions
{
    internal delegate void UpdateAvailableSessionDataEventHandler();

    internal delegate void SetAvailableSessionDataEventHandler(string data);

    internal class ReportsController : IReportsController
    {
        public event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;

        public event SetAvailableSessionDataEventHandler SetUserDefinedTargetData;

        public void SendUpdateAvailableSessionDataRequest()
        {
            UpdateAvailableSessionData?.Invoke();
        }

        public void SendSetUserDefinedTargetDataRequest(string data)
        {
            SetUserDefinedTargetData?.Invoke(data);
        }
    }
}
