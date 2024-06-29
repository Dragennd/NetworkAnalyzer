using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TimeStampHandler
    {
        // Return the current datetime 
        public async Task<DateTime> CalculateTimeStampAsync()
        {
            return await Task.FromResult(DateTime.Now);
        }
    }
}
