using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class UserTargetsHandler
    {
        private string TargetName { get; set; }
        private string UserDefinedTarget { get; set; }
        private string Latency { get; set; }
    }
}
