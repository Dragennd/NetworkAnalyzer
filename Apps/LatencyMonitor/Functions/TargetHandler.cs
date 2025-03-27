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
    internal class TargetHandler
    {
        private string TargetName { get; set; }
        private string UserDefinedTarget { get; set; }
        private int Hop { get; set; }
        private LatencyMonitorData Data { get; set; }
        private LatencyMonitorTargetStatus Status { get; set; }

        public TargetHandler(
            [Optional]string targetName,
            [Optional]string userDefinedTarget,
            [Optional]int hop,
            [Optional]LatencyMonitorData data,
            [Optional]LatencyMonitorTargetStatus status)
        {
            TargetName = targetName;
            UserDefinedTarget = userDefinedTarget;
            Hop = hop;
            Data = data;
            Status = status;
        }

        public async Task<LatencyMonitorData> NewInitialTargetDataAsync()
        {
            PingReply response;
            Stopwatch sw = new();

            using (var ping = new Ping())
            {
                sw = Stopwatch.StartNew();
                response = await ping.SendPingAsync(TargetName, 4000, new byte[32]);
                sw.Stop();
            }

            var targetData = new LatencyMonitorData()
            {
                TargetName = TargetName,
                UserDefinedTarget = UserDefinedTarget,
                DNSHostName = await ResolveIPAddressFromDNSAsync(TargetName, Status),
                Latency = ProcessInitialLatency(sw.Elapsed.TotalMilliseconds.ToString()),
                LowestLatency = ProcessInitialLatency(sw.Elapsed.TotalMilliseconds.ToString()),
                HighestLatency = ProcessInitialLatency(sw.Elapsed.TotalMilliseconds.ToString()),
                AverageLatency = ProcessInitialLatency(sw.Elapsed.TotalMilliseconds.ToString()),
                TotalPacketsLost = await CalculateTotalPacketsLostAsync(TargetName, response.Status),
                Hop = Hop,
                AverageLatencyCounter = await CalculateAverageLatencyCounterAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                TotalLatency = await CalculateTotalLatencyAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                FailedPing = await CalculateFailedPingAsync(TargetName, response.Status),
                TimeStamp = await CalculateTimeStampAsync()
            };

            return targetData;
        }

        public async Task<LatencyMonitorData> NewTargetDataAsync()
        {
            PingReply response;
            Stopwatch sw = new();

            using (var ping = new Ping())
            {
                sw = Stopwatch.StartNew();
                response = await ping.SendPingAsync(TargetName, 4000, new byte[32]);
                sw.Stop();
            }

            var targetData = new LatencyMonitorData()
            {
                TargetName = Data.TargetName,
                UserDefinedTarget = Data.UserDefinedTarget,
                DNSHostName = Data.DNSHostName,
                Latency = sw.Elapsed.TotalMilliseconds.ToString(),
                LowestLatency = await CalculateLowestLatencyAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                HighestLatency = await CalculateHighestLatencyAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                AverageLatency = await CalculateAverageLatencyAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                TotalPacketsLost = await CalculateTotalPacketsLostAsync(TargetName, response.Status),
                Hop = Data.Hop,
                AverageLatencyCounter = await CalculateAverageLatencyCounterAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                TotalLatency = await CalculateTotalLatencyAsync(TargetName, (int)sw.Elapsed.TotalMilliseconds, response.Status),
                FailedPing = await CalculateFailedPingAsync(TargetName, response.Status),
                TimeStamp = await CalculateTimeStampAsync()
            };

            return targetData;
        }

        #region Private Methods
        private string ProcessInitialLatency(string latency)
        {
            if (Status == LatencyMonitorTargetStatus.Active)
            {
                return latency;
            }
            else
            {
                return "-";
            }
        }
        #endregion Private Methods
    }
}
