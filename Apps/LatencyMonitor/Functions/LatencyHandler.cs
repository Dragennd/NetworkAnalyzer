using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal static class LatencyHandler
    {
        public static async Task<string> CalculateLatencyAsync(int latency) =>
            await Task.FromResult(latency.ToString());

        public static async Task<string> CalculateLowestLatencyAsync(int latency, IPStatus ipStatus, LatencyMonitorData data)
        {
            string response;

            if (ipStatus == IPStatus.Success && latency <= int.Parse(data.LowestLatency))
            {
                response = latency.ToString();
            }
            else
            {
                response = data.LowestLatency;
            }

            return await Task.FromResult(response);
        }

        public static async Task<string> CalculateHighestLatencyAsync(int latency, IPStatus ipStatus, LatencyMonitorData data)
        {
            string response;

            if (ipStatus == IPStatus.Success && latency >= int.Parse(data.HighestLatency))
            {
                response = latency.ToString();
            }
            else
            {
                response = data.HighestLatency;
            }

            return await Task.FromResult(response);
        }

        public static async Task<string> CalculateAverageLatencyAsync(IPStatus ipStatus, LatencyMonitorData data)
        {
            string response;

            if (ipStatus == IPStatus.Success)
            {
                response = (data.TotalLatency / data.AverageLatencyCounter).ToString();
            }
            else
            {
                response = data.AverageLatency;
            }

            return await Task.FromResult(response);
        }

        public static async Task<int> CalculateAverageLatencyCounterAsync(IPStatus ipStatus, LatencyMonitorData data)
        {
            int response;

            if (ipStatus == IPStatus.Success)
            {
                response = data.AverageLatencyCounter + 1;
            }
            else
            {
                response = data.AverageLatencyCounter;
            }

            return await Task.FromResult(response);
        }

        public static async Task<int> CalculateTotalLatencyAsync(int latency, LatencyMonitorData data) =>
            await Task.FromResult(data.TotalLatency + latency);
    }
}
