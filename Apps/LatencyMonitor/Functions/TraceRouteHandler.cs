using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        private string TargetName { get; set; }
        private List<LatencyMonitorData> TargetData { get; set; }

        public TracerouteHandler()
        {
            TargetData = new();
        }
    }
}
