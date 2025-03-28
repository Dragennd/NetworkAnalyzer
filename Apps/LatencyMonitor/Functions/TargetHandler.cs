using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.IPScanner.Functions.DNSHandler;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TargetHandler
    {
        private string TargetName { get; set; }
        private string UserDefinedTarget { get; set; }
        private int Hop { get; set; }
        private bool Init { get; set; }
        private LatencyMonitorData Data { get; set; }
        private LatencyMonitorTargetStatus Status { get; set; }

        public TargetHandler(
            [Optional]string targetName,
            [Optional]string userDefinedTarget,
            [Optional]int hop,
            [Optional]bool init,
            [Optional]LatencyMonitorData data,
            [Optional]LatencyMonitorTargetStatus status)
        {
            TargetName = targetName;
            UserDefinedTarget = userDefinedTarget;
            Hop = hop;
            Data = data;
            Status = status;
            Init = init;
        }

        public async Task<LatencyMonitorData> NewTargetDataAsync()
        {
            var targetData = new LatencyMonitorData();
            PingReply response;

            using (var ping = new Ping())
            {
                response = await ping.SendPingAsync(TargetName, 4000, new byte[32]);
            }

            targetData.TargetName = TargetName;
            targetData.UserDefinedTarget = UserDefinedTarget;
            targetData.DNSHostName = await ResolveIPAddressFromDNSAsync(TargetName, Status);
            targetData.Latency = await CalculateLatencyAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.LowestLatency = await CalculateLowestLatencyAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.HighestLatency = await CalculateHighestLatencyAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.AverageLatency = await CalculateAverageLatencyAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.TotalPacketsLost = await CalculateTotalPacketsLostAsync(Init, Status, response.Status);
            targetData.Hop = Hop;
            targetData.AverageLatencyCounter = await CalculateAverageLatencyCounterAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.FailedPing = await CalculateFailedPingAsync(response.Status);
            targetData.TotalLatency = await CalculateTotalLatencyAsync((int)response.RoundtripTime, Init, Status, response.Status, Data);
            targetData.TimeStamp = await CalculateTimeStampAsync();

            return targetData;
        }
        #region Private Methods
        
        #endregion Private Methods
    }
}
