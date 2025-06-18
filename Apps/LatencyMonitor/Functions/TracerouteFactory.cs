using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteFactory : ITracerouteFactory
    {
        private readonly ILatencyMonitorController _latencyMonitorController;

        public TracerouteFactory(ILatencyMonitorController latencyMonitorController)
        {
            _latencyMonitorController = latencyMonitorController;
        }

        public TracerouteWorker Create(string target)
        {
            return new TracerouteWorker(target, _latencyMonitorController);
        }
    }
}
