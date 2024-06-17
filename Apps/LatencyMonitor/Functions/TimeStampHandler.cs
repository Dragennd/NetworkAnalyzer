using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class TimeStampHandler
    {
        // Calculate the timestamp of the last major change to occur (changing to down or unstable generally)
        // and write that to the dictionaries
        public async Task<DateTime> CalculateTimeStampAsync(string targetName, LatencyMonitorSessionStatus status, LatencyMonitorSessionType type, bool initialization)
        {
            return await Task.FromResult(DateTime.Now);
        }
    }
}
