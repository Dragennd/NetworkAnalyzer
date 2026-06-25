using System.Collections.Generic;
using NetworkAnalyzer_UI_Test.Interfaces;
using NetworkAnalyzer_UI_Test.Models;

namespace NetworkAnalyzer_UI_Test.EventControllers
{
    internal delegate void LatencyMonitorDataEventHandler(LatencyMonitorData data);
    internal delegate void LatencyMonitorNumEventHandler(int num);
    internal delegate void LatencyMonitorStringEventHandler(string data);
    internal delegate void LatencyMonitorEmergencyStopEventHandler(bool stop);
    internal delegate void LatencyMonitorErrorMessageEventHandler(LogType logType, string message);
    internal delegate void LatencyMonitorHistoryDataEventHandler(List<LatencyMonitorReportEntries> data);
    internal delegate void LatencyMonitorSessionStatusEventHandler(LatencyMonitorSessionStatus status);

    internal class LatencyMonitorController : ILatencyMonitorController
    {
        public event LatencyMonitorDataEventHandler? SetSelectedTargetData;
        public event LatencyMonitorDataEventHandler? SetLiveTargetData;
        public event LatencyMonitorDataEventHandler? SetTracerouteData;
        public event LatencyMonitorDataEventHandler? UpdateLiveTargetData;
        public event LatencyMonitorDataEventHandler? UpdateTracerouteData;
        public event LatencyMonitorDataEventHandler? SetTracerouteTargets;
        public event LatencyMonitorStringEventHandler? SetSelectedTargetGuid;
        public event LatencyMonitorHistoryDataEventHandler? SetHistoryData;
        public event LatencyMonitorEmergencyStopEventHandler? SetStopCode;
        public event LatencyMonitorErrorMessageEventHandler? SetErrorMessage;
        public event LatencyMonitorSessionStatusEventHandler? SetSessionStatus;

        public void SendSetSelectedTargetRequest(LatencyMonitorData data)
        {
            SetSelectedTargetData?.Invoke(data);
        }

        public void SendSetLiveTargetRequest(LatencyMonitorData data)
        {
            SetLiveTargetData?.Invoke(data);
        }

        public void SendSetTracerouteRequest(LatencyMonitorData data)
        {
            SetTracerouteData?.Invoke(data);
        }

        public void SendUpdateLiveTargetRequest(LatencyMonitorData data)
        {
            UpdateLiveTargetData?.Invoke(data);
        }

        public void SendUpdateTracerouteRequest(LatencyMonitorData data)
        {
            UpdateTracerouteData?.Invoke(data);
        }

        public void SendSetSelectedTargetGUIDRequest(string data)
        {
            SetSelectedTargetGuid?.Invoke(data);
        }

        public void SendStopCodeRequest(bool stop)
        {
            SetStopCode?.Invoke(stop);
        }

        public void SendErrorMessage(LogType logType, string message)
        {
            SetErrorMessage?.Invoke(logType, message);
        }

        public void SendSetTracerouteTargetsRequest(LatencyMonitorData data)
        {
            SetTracerouteTargets?.Invoke(data);
        }

        public void SendHistoryDataRequest(List<LatencyMonitorReportEntries> data)
        {
            SetHistoryData?.Invoke(data);
        }

        public void SendSetSessionStatusRequest(LatencyMonitorSessionStatus status)
        {
            SetSessionStatus?.Invoke(status);
        }
    }
}
