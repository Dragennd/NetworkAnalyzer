using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
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

        public TracerouteWorker Create(string target, string reportID)
        {
            var dnsHandler = App.AppHost.Services.GetRequiredService<IDNSHandler>();

            return new TracerouteWorker(target, reportID, _latencyMonitorController, dnsHandler);
        }
    }
}
