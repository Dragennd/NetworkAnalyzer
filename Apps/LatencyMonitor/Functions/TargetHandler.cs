using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
        private string FriendlyName { get; set; }
        private string DnsHostName { get; set; }
        private int Hop { get; set; }
        private bool Init { get; set; }
        private LatencyMonitorData Data { get; set; }
        private LatencyMonitorTargetStatus Status { get; set; }

        public TargetHandler(
            [Optional]string targetName,
            [Optional]string userDefinedTarget,
            [Optional]string friendlyName,
            [Optional]string dnsHostName,
            [Optional]int hop,
            [Optional]bool init,
            [Optional]LatencyMonitorData data,
            [Optional]LatencyMonitorTargetStatus status)
        {
            TargetName = targetName;
            UserDefinedTarget = userDefinedTarget;
            DnsHostName = dnsHostName;
            FriendlyName = friendlyName;
            Hop = hop;
            Data = data;
            Status = status;
            Init = init;

            if (Data != null)
            {
                TargetName = Data.TargetName;
                UserDefinedTarget = Data.UserDefinedTarget;
                FriendlyName = Data.SpecifiedTargetName;
                DnsHostName = Data.DNSHostName;
                Hop = Data.Hop;
                Status = Data.TargetStatus;
            }
        }

        public async Task<LatencyMonitorData> NewTargetDataAsync()
        {
            var targetData = new LatencyMonitorData();
            PingReply response;
            int rtt = 0;
            IPStatus ips = IPStatus.Unknown;

            if (TargetName != "Request timed out")
            {
                using (var ping = new Ping())
                {
                    response = await ping.SendPingAsync(TargetName, 4000, new byte[32]);
                    rtt = (int)response.RoundtripTime;
                    ips = response.Status;
                }
            }

            targetData.TargetName = TargetName;
            targetData.UserDefinedTarget = UserDefinedTarget;
            targetData.DNSHostName = DnsHostName;
            targetData.SpecifiedTargetName = await SetSpecifiedTargetName();
            targetData.Latency = await CalculateLatencyAsync(rtt, Init, Status, ips, Data);
            targetData.LowestLatency = await CalculateLowestLatencyAsync(rtt, Init, Status, ips, Data);
            targetData.HighestLatency = await CalculateHighestLatencyAsync(rtt, Init, Status, ips, Data);
            targetData.AverageLatency = await CalculateAverageLatencyAsync(rtt, Init, Status, ips, Data);
            targetData.TotalPacketsLost = await CalculateTotalPacketsLostAsync(Init, Status, ips, Data);
            targetData.Hop = Hop;
            targetData.TargetStatus = Status;
            targetData.AverageLatencyCounter = await CalculateAverageLatencyCounterAsync(rtt, Init, Status, ips, Data);
            targetData.FailedPing = await CalculateFailedPingAsync(ips);
            targetData.TotalLatency = await CalculateTotalLatencyAsync(rtt, Init, Status, ips, Data);
            targetData.TimeStamp = await CalculateTimeStampAsync();

            return targetData;
        }
        #region Private Methods
        private async Task<string> SetSpecifiedTargetName()
        {
            string friendlyName = "N/A";

            if (DnsHostName == UserDefinedTarget)
            {
                friendlyName = FriendlyName;
            }

            return await Task.FromResult(friendlyName);
        }
        #endregion Private Methods
    }
}
