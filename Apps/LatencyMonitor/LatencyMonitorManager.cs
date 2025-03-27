using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal static class LatencyMonitorManager
    {
        public static async Task<List<LatencyMonitorData>> ExecuteInitialSessionAsync(List<string> targetList)
        {
            List<LatencyMonitorData> TracerouteResults = new();

            foreach (var a in targetList)
            {
                var tr = new TracerouteHandler(a);
                TracerouteResults.AddRange(await tr.NewTracerouteDataAsync());
            }

            return TracerouteResults;
        }

        public static async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
        {
            var u = new TargetHandler(data: data);

            return await u.NewTargetDataAsync();
        }
    }
}
