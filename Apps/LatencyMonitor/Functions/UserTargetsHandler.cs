using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Diagnostics;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class UserTargetsHandler
    {
        private string TargetName { get; set; }
        private string UserDefinedTarget { get; set; }
        private int Hop { get; set; }
        private LatencyMonitorData Data { get; set; }

        public UserTargetsHandler(string targetName, string userDefinedTarget, [Optional]int hop, [Optional]LatencyMonitorData? data)
        {
            TargetName = targetName;
            UserDefinedTarget = userDefinedTarget;
            Hop = hop;

            if (data != null)
            {
                Data = data;
            }
        }

        public async Task<LatencyMonitorData> NewUserTargetDataAsync()
        {
            PingReply response;
            Stopwatch sw = new();

            if (TargetName != "Request timed out")
            {
                using (var ping = new Ping())
                {
                    sw = Stopwatch.StartNew();
                    response = await ping.SendPingAsync(TargetName, 4000, new byte[32]);
                    sw.Stop();
                }
            }

            if (Data == null && TargetName != "Request timed out")
            {
                var targetData = new LatencyMonitorData()
                {
                    TargetName = TargetName,
                    UserDefinedTarget = UserDefinedTarget,
                    DNSHostName = await ResolveIPAddressFromDNSAsync(TargetName),
                    Latency = sw.Elapsed.TotalMilliseconds.ToString(),
                    LowestLatency = sw.Elapsed.TotalMilliseconds.ToString(),
                    HighestLatency = sw.Elapsed.TotalMilliseconds.ToString(),
                    AverageLatency = sw.Elapsed.TotalMilliseconds.ToString(),
                    TotalPacketsLost = ,
                    Hop = ,
                    FailedHopCounter = ,
                    AverageLatencyCounter = ,
                    TotalLatency = (int)sw.Elapsed.TotalMilliseconds,
                    FailedPing = ,
                    TimeStamp = 
                };
            }
            else if (TargetName == "Request timed out")
            {
                var targetData = new LatencyMonitorData()
                {
                    TargetName = TargetName,
                    UserDefinedTarget = UserDefinedTarget,
                    DNSHostName = "N/A",
                    Latency = "-",
                    LowestLatency = "-",
                    HighestLatency = "-",
                    AverageLatency = "-",
                    TotalPacketsLost = "-",
                    Hop = Hop,
                    FailedHopCounter = 0,
                    AverageLatencyCounter = 0,
                    TotalLatency = 0,
                    FailedPing = true,
                    TimeStamp = DateTime.Now
                };
            }
            else
            {
                var targetData = new LatencyMonitorData()
                {
                    TargetName = TargetName,
                    UserDefinedTarget = UserDefinedTarget,
                    DNSHostName = Data.DNSHostName,
                    Latency = sw.Elapsed.TotalMilliseconds.ToString(),
                    LowestLatency = ,
                    HighestLatency = ,
                    AverageLatency = ,
                    TotalPacketsLost = ,
                    Hop = Data.Hop,
                    FailedHopCounter = ,
                    AverageLatencyCounter = ,
                    TotalLatency = ,
                    FailedPing = ,
                    TimeStamp =
                };
            }
        }

        #region Private Methods
        
        #endregion Private Methods
    }
}
