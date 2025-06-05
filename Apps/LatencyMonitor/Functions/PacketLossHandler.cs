using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal static class PacketLossHandler
    {
        public static async Task<string> CalculateTotalPacketsLostAsync(IPStatus ipStatus, LatencyMonitorData data)
        {
            string response;

            if (ipStatus != IPStatus.Success)
            {
                response = data.TotalPacketsLost + 1;
            }
            else
            {
                response = data.TotalPacketsLost;
            }

            return await Task.FromResult(response);
        }

        public static async Task<bool> CalculateFailedPingAsync(IPStatus ipStatus)
        {
            bool response;

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
