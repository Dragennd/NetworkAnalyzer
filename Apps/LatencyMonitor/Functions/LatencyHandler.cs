using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal static class LatencyHandler
    {
        public static async Task<string> CalculateLatencyAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            string response = string.Empty;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = latency.ToString();
                }
                else
                {
                    response = "-";
                }
            }
            else
            {
                response = latency.ToString();
            }

                return await Task.FromResult(response);
        }

        public static async Task<string> CalculateLowestLatencyAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            string response = string.Empty;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = latency.ToString();
                }
                else
                {
                    response = "-";
                }
            }
            else
            {
                if (ipStatus == IPStatus.Success && latency <= int.Parse(data.LowestLatency))
                {
                    response = latency.ToString();
                }
                else
                {
                    response = data.LowestLatency;
                }
            }

            return await Task.FromResult(response);
        }

        public static async Task<string> CalculateHighestLatencyAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            string response = string.Empty;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = latency.ToString();
                }
                else
                {
                    response = "-";
                }
            }
            else
            {
                if (ipStatus == IPStatus.Success && latency >= int.Parse(data.HighestLatency))
                {
                    response = latency.ToString();
                }
                else
                {
                    response = data.HighestLatency;
                }
            }

            return await Task.FromResult(response);
        }

        public static async Task<string> CalculateAverageLatencyAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            string response = string.Empty;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = latency.ToString();
                }
                else
                {
                    response = "-";
                }
            }
            else
            {
                if (ipStatus == IPStatus.Success)
                {
                    response = (data.TotalLatency / data.AverageLatencyCounter).ToString();
                }
                else
                {
                    response = data.AverageLatency;
                }
            }

            return await Task.FromResult(response);
        }

        public static async Task<int> CalculateAverageLatencyCounterAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            int response = 0;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active && ipStatus == IPStatus.Success)
                {
                    response = 1;
                }
                else
                {
                    response = 0;
                }
            }
            else
            {
                if (ipStatus == IPStatus.Success)
                {
                    response = data.AverageLatencyCounter + 1;
                }
                else
                {
                    response = data.AverageLatencyCounter;
                }
            }

            return await Task.FromResult(response);
        }

        public static async Task<int> CalculateTotalLatencyAsync(int latency, bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            int response = 0;

            if (init)
            {
                if (tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = latency;
                }
                else
                {
                    response = 0;
                }
            }
            else
            {
                response = data.TotalLatency + latency;
            }

            return await Task.FromResult(response);
        }
    }
}
