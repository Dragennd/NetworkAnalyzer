using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Interfaces
{
    internal interface IReportsController
    {
        event UpdateAvailableSessionDataEventHandler UpdateAvailableSessionData;
        event SetAvailableSessionDataEventHandler SetUserDefinedTargetData;

        void SendUpdateAvailableSessionDataRequest();
        void SendSetUserDefinedTargetDataRequest(string data);
    }
}
