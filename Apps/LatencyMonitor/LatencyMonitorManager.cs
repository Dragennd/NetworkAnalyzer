using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal static class LatencyMonitorManager
    {
        public static async Task ExecuteInitialSessionAsync(List<string> targetList)
        {
            //List<LatencyMonitorData> TracerouteResults = new();

            foreach (var a in targetList)
            {
                var tr = new TracerouteHandler(a);
                await tr.NewTracerouteDataAsync();
                //TracerouteResults.AddRange(await tr.NewTracerouteDataAsync());
            }
        }

        public static async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
        {
            var u = new TargetHandler(data: data);

            return await u.UpdateTargetDataAsync();
        }
    }
}
