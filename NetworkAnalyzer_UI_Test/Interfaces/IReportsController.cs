using NetworkAnalyzer_UI_Test.EventControllers;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface IReportsController
    {
        event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;
        event SetAvailableSessionDataEventHandler SetUserDefinedTargetData;

        void SendUpdateAvailableSessionDataRequest();
        void SendSetUserDefinedTargetDataRequest(string data);
    }
}
