using NetworkAnalyzer_UI_Test.Interfaces;

namespace NetworkAnalyzer_UI_Test.EventControllers
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
