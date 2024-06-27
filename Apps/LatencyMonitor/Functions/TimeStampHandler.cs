using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TimeStampHandler
    {
        // Calculate the timestamp of the last major change to occur (changing to down or unstable generally)
        // and write that to the dictionaries
        public async Task<DateTime> CalculateTimeStampAsync(string targetName, LatencyMonitorSessionStatus status, LatencyMonitorSessionType type, bool initialization)
        {
            return await Task.FromResult(DateTime.Now);
        }
    }
}
