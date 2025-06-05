using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal delegate void LatencyMonitorControllerEventhandler(LatencyMonitorData data);

    internal class LatencyMonitorController
    {
        public static event LatencyMonitorControllerEventhandler LiveUpdate;

        public static event LatencyMonitorControllerEventhandler LiveTargetsSet;

        public static event LatencyMonitorControllerEventhandler TracerouteSet;

        public static void SendLiveUpdateRequest(LatencyMonitorData data)
        {
            LiveUpdate.Invoke(data);
        }

        public static void SendLiveTargetsSetRequest(LatencyMonitorData data)
        {
            LiveTargetsSet.Invoke(data);
        }

        public static void SendTracerouteSetRequest(LatencyMonitorData data)
        {
            TracerouteSet.Invoke(data);
        }
    }
}
