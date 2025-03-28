using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal static class PacketLossHandler
    {
        public static async Task<string> CalculateTotalPacketsLostAsync(bool init, LatencyMonitorTargetStatus tStatus, IPStatus ipStatus, [Optional]LatencyMonitorData data)
        {
            string response = string.Empty;

            if (init)
            {
                if (ipStatus != IPStatus.Success && tStatus == LatencyMonitorTargetStatus.Active)
                {
                    response = "1";
                }
                else if (ipStatus != IPStatus.Success && tStatus != LatencyMonitorTargetStatus.Active)
                {
                    response = "-";
                }
                else
                {
                    response = "0";
                }
            }
            else
            {
                if (ipStatus != IPStatus.Success)
                {
                    response = data.TotalPacketsLost + 1;
                }
                else
                {
                    response = data.TotalPacketsLost;
                }
            }

            return await Task.FromResult(response);
        }

        public static async Task<bool> CalculateFailedPingAsync(IPStatus ipStatus)
        {
            bool response = false;

            if (ipStatus == IPStatus.Success)
            {
                response = false;
            }
            else
            {
                response = true;
            }

            return await Task.FromResult(response);
        }
    }
}
