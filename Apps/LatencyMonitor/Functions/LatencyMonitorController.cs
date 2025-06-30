using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal delegate void LatencyMonitorDataEventHandler(LatencyMonitorData data);

    internal delegate void LatencyMonitorNumEventHandler(int num);

    internal delegate void LatencyMonitorEmergencyStop(bool stop);

    internal delegate void LatencyMonitorErrorMessage(LogType logType, string message);

    internal class LatencyMonitorController : ILatencyMonitorController
    {
        public event LatencyMonitorDataEventHandler SetSelectedTargetData;

        public event LatencyMonitorDataEventHandler SetLiveTargetData;

        public event LatencyMonitorDataEventHandler SetTracerouteData;

        public event LatencyMonitorDataEventHandler UpdateLiveTargetData;

        public event LatencyMonitorDataEventHandler UpdateTracerouteData;

        public event LatencyMonitorNumEventHandler UpdatePacketsSent;

        public event LatencyMonitorEmergencyStop SetStopCode;

        public event LatencyMonitorErrorMessage SetErrorMessage;

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

        public void SendUpdatePacketsSentRequest(int num)
        {
            UpdatePacketsSent?.Invoke(num);
        }

        public void SendStopCodeRequest(bool stop)
        {
            SetStopCode?.Invoke(stop);
        }

        public void SendErrorMessage(LogType logType, string message)
        {
            SetErrorMessage?.Invoke(logType, message);
        }
    }
}
