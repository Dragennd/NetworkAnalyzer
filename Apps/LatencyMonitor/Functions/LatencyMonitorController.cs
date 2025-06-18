using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal delegate void LatencyMonitorDataEventHandler(LatencyMonitorData data);

    internal delegate void LatencyMonitorNumEventHandler(int num);

    internal class LatencyMonitorController : ILatencyMonitorController
    {
        public event LatencyMonitorDataEventHandler SetSelectedTargetData;

        public event LatencyMonitorDataEventHandler SetLiveTargetData;

        public event LatencyMonitorDataEventHandler SetTracerouteData;

        public event LatencyMonitorDataEventHandler UpdateLiveTargetData;

        public event LatencyMonitorDataEventHandler UpdateTracerouteData;

        public event LatencyMonitorNumEventHandler UpdatePacketsSent;

        public void SendSetSelectedTargetRequest(LatencyMonitorData data)
        {
            SetSelectedTargetData.Invoke(data);
        }

        public void SendSetLiveTargetRequest(LatencyMonitorData data)
        {
            SetLiveTargetData.Invoke(data);
        }

        public void SendSetTracerouteRequest(LatencyMonitorData data)
        {
            SetTracerouteData.Invoke(data);
        }

        public void SendUpdateLiveTargetRequest(LatencyMonitorData data)
        {
            UpdateLiveTargetData.Invoke(data);
        }

        public void SendUpdateTracerouteRequest(LatencyMonitorData data)
        {
            UpdateTracerouteData.Invoke(data);
        }

        public void SendUpdatePacketsSentRequest(int num)
        {
            UpdatePacketsSent.Invoke(num);
        }
    }
}
