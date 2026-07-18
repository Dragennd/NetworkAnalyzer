using System.Collections.Generic;
using NetworkAnalyzer_UI_Test.EventControllers;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.Interfaces
{
    internal interface ILatencyMonitorController
    {
        event LatencyMonitorDataEventHandler SetSelectedTargetData;
        event LatencyMonitorDataEventHandler SetLiveTargetData;
        event LatencyMonitorDataEventHandler SetTracerouteData;
        event LatencyMonitorDataEventHandler UpdateLiveTargetData;
        event LatencyMonitorDataEventHandler UpdateTracerouteData;
        event LatencyMonitorDataEventHandler SetTracerouteTargets;
        event LatencyMonitorStringEventHandler SetSelectedTargetGuid;
        event LatencyMonitorEmergencyStopEventHandler SetStopCode;
        event LatencyMonitorErrorMessageEventHandler SetErrorMessage;
        event LatencyMonitorHistoryDataEventHandler SetHistoryData;
        event LatencyMonitorSessionStatusEventHandler? SetSessionStatus;

        void SendSetSelectedTargetRequest(LatencyMonitorData data);
        void SendSetLiveTargetRequest(LatencyMonitorData data);
        void SendSetTracerouteRequest(LatencyMonitorData data);
        void SendUpdateLiveTargetRequest(LatencyMonitorData data);
        void SendUpdateTracerouteRequest(LatencyMonitorData data);
        void SendSetSelectedTargetGUIDRequest(string data);
        void SendStopCodeRequest(bool stop);
        void SendErrorMessage(LogType logType, string message);
        void SendSetTracerouteTargetsRequest(LatencyMonitorData data);
        void SendHistoryDataRequest(List<LatencyMonitorReportEntries> data);
        void SendSetSessionStatusRequest(LatencyMonitorSessionStatus status);
    }
}
