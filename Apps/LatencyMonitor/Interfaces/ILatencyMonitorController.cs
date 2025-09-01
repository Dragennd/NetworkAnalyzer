using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Interfaces
{
    internal interface ILatencyMonitorController
    {
        event LatencyMonitorDataEventHandler SetSelectedTargetData;
        event LatencyMonitorDataEventHandler SetLiveTargetData;
        event LatencyMonitorDataEventHandler SetTracerouteData;
        event LatencyMonitorDataEventHandler UpdateLiveTargetData;
        event LatencyMonitorDataEventHandler UpdateTracerouteData;
        event LatencyMonitorDataEventHandler SetTracerouteTargets;
        event LatencyMonitorEmergencyStopEventHandler SetStopCode;
        event LatencyMonitorErrorMessageEventHandler SetErrorMessage;
        event LatencyMonitorHistoryDataEventHandler SetHistoryData;

        void SendSetSelectedTargetRequest(LatencyMonitorData data);
        void SendSetLiveTargetRequest(LatencyMonitorData data);
        void SendSetTracerouteRequest(LatencyMonitorData data);
        void SendUpdateLiveTargetRequest(LatencyMonitorData data);
        void SendUpdateTracerouteRequest(LatencyMonitorData data);
        void SendStopCodeRequest(bool stop);
        void SendErrorMessage(LogType logType, string message);
        void SendSetTracerouteTargetsRequest(LatencyMonitorData data);
        void SendHistoryDataRequest(List<LatencyMonitorReportEntries> data);
    }
}
