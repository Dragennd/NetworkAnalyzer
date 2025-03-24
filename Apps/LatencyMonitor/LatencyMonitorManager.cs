using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal static class LatencyMonitorManager
    {
        public static async Task<List<LatencyMonitorData>> ExecuteInitialSessionAsync(string target)
        {
            // Performs initial traceroute and data gathering to
            // populate the AllTargets collection in the LatencyMonitorViewModel
        }

        public static async Task<LatencyMonitorData> ExecuteSessionUpdateAsync(LatencyMonitorData data)
        {
            // Performs followup tests to update the targets listed
            // in the LiveTargets collection in the LatencyMonitorViewModel
        }
    }
}
