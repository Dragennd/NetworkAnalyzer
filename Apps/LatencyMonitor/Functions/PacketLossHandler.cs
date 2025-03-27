using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;
using System.Security.Policy;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal static class PacketLossHandler
    {
        public static async Task<string> CalculateTotalPacketsLostAsync(string targetName, IPStatus status)
        {

        }

        public static async Task<bool> CalculateFailedPingAsync(string targetName, IPStatus status)
        {

        }
    }
}
