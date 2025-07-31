using NetworkAnalyzer.Apps.LatencyMonitor.Functions;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Interfaces
{
    internal interface ITracerouteFactory
    {
        TracerouteWorker Create(string target, string reportID);
    }
}
