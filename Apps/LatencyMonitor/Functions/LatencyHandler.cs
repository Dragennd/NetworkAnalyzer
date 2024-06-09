using System.Net.NetworkInformation;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class LatencyHandler
    {
        public static async Task<int> CalculateLowestLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int lowestLatency = 0;

            if (status == IPStatus.Success && latency <= lastDataSet.LowestLatency)
            {
                lowestLatency = latency;
            }
            else
            {
                lowestLatency = lastDataSet.LowestLatency;
            }

            return await Task.FromResult(lowestLatency);
        }

        public static async Task<int> CalculateHighestLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int highestLatency = 0;

            if (status == IPStatus.Success && latency >= lastDataSet.HighestLatency)
            {
                highestLatency = latency;
            }
            else
            {
                highestLatency = lastDataSet.HighestLatency;
            }

            return await Task.FromResult(highestLatency);
        }

        public static async Task<int> CalculateAverageLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int averageLatency = 0;

            if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
            {
                averageLatency = lastDataSet.AllAveragesCombined / lastDataSet.AverageLatencyDivider;
            }
            else if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency == 0)
            {
                averageLatency = latency;
            }
            else
            {
                averageLatency = lastDataSet.AverageLatency;
            }

            return await Task.FromResult(averageLatency);
        }

        public static async Task<int> CalculateCounterAsync(IPStatus status, long latency, string ipAddress)
        {
            int averageLatencyDivider = 0;

            if (!LiveData.ContainsKey(ipAddress))
            {
                averageLatencyDivider = 1;
            }
            else
            {
                var lastDataSet = LiveData[ipAddress].LastOrDefault();

                if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
                {
                    averageLatencyDivider = lastDataSet.AverageLatencyDivider + 1;
                }
                else
                {
                    averageLatencyDivider = lastDataSet.AverageLatencyDivider;
                }
            }

            return await Task.FromResult(averageLatencyDivider);
        }
    }
}
