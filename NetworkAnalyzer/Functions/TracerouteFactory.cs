using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.Functions
{
    internal class TracerouteFactory : ITracerouteFactory
    {
        private readonly LatencyMonitorController _latencyMonitorController;

        public TracerouteFactory(LatencyMonitorController latencyMonitorController)
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
