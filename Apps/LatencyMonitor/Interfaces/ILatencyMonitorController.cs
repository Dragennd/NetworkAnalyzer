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
        event LatencyMonitorNumEventHandler UpdatePacketsSent;

        void SendSetSelectedTargetRequest(LatencyMonitorData data);
        void SendSetLiveTargetRequest(LatencyMonitorData data);
        void SendSetTracerouteRequest(LatencyMonitorData data);
        void SendUpdateLiveTargetRequest(LatencyMonitorData data);
        void SendUpdateTracerouteRequest(LatencyMonitorData data);
        void SendUpdatePacketsSentRequest(int num);
    }
}
