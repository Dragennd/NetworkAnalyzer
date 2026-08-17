using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Interfaces;

namespace NetworkAnalyzer.Functions
{
    internal class TracerouteFactory : ITracerouteFactory
    {
        public TracerouteWorker Create(string target, string reportID)
        {
            var dnsHandler = App.AppHost.Services.GetRequiredService<IDNSHandler>();

            return new TracerouteWorker(target, reportID, dnsHandler);
        }
    }
}
