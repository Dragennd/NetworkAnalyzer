using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TargetWorker
    {
        private string ReportID { get; set; }
        private string DisplayName { get; set; }
        private string TargetName { get; set; }
        private string TargetAddress { get; set; }
        private string TracerouteGUID { get; set; }
        private string TargetGUID { get; set; }
        private bool IsUserDefinedTarget { get; set; }
        private int Hop { get; set; }
        private LatencyMonitorData Data { get; set; }
        private LatencyMonitorTargetStatus Status { get; set; }

        public TargetWorker(
            string reportID,
            [Optional]string displayName,
            [Optional]string targetName,
            [Optional]string targetAddress,
            [Optional]string tracerouteGUID,
            [Optional]bool isUserDefinedTarget,
            [Optional]int hop,
            [Optional]LatencyMonitorData data,
            [Optional]LatencyMonitorTargetStatus status)
        {
            ReportID = reportID;
            DisplayName = displayName;
            TargetName = targetName;
            TargetAddress = targetAddress;
            TracerouteGUID = tracerouteGUID;
            TargetGUID = Guid.NewGuid().ToString();
            IsUserDefinedTarget = isUserDefinedTarget;
            Hop = hop;
            Data = data;
            Status = status;

            if (Data != null)
            {
                DisplayName = Data.DisplayName;
                TargetName = Data.TargetName;
                TargetAddress = Data.TargetAddress;
                TracerouteGUID = Data.TracerouteGUID;
                TargetGUID = Data.TargetGUID;
                Status = Data.TargetStatus;
                Hop = Data.Hop;
            }
        }

        public async Task<LatencyMonitorData> NewTargetDataAsync()
        {
            var targetData = new LatencyMonitorData();
            PingReply response;
            int rtt = 0;
            IPStatus ips = IPStatus.Unknown;

            if (Status == LatencyMonitorTargetStatus.Active)
            {
                using (var ping = new Ping())
                {
                    //response = await ping.SendPingAsync(TargetAddress, 4000, new byte[32]);
                    //rtt = (int)response.RoundtripTime;
                    //ips = response.Status;
                    try
                    {
                        response = await ping.SendPingAsync(TargetAddress, 4000, new byte[32]);
                        rtt = (int)response.RoundtripTime;
                        ips = response.Status;
                    }
                    catch (PingException)
                    {
                        rtt = 0;
                        ips = IPStatus.Unknown;
                        Status = LatencyMonitorTargetStatus.NoResponse;
                    }
                }

                targetData.Latency = await CalculateLatencyAsync(rtt);
                targetData.LowestLatency = await CalculateLatencyAsync(rtt);
                targetData.HighestLatency = await CalculateLatencyAsync(rtt);
                targetData.AverageLatency = await CalculateLatencyAsync(rtt);
                targetData.TotalPacketsLost = "0";
                targetData.AverageLatencyCounter = 1;
                targetData.FailedPing = false;
                targetData.TotalLatency = int.Parse(await CalculateLatencyAsync(rtt));
            }

            targetData.ReportID = ReportID;
            targetData.DisplayName = DisplayName;
            targetData.TargetName = TargetName;
            targetData.TargetAddress = TargetAddress;
            targetData.IsUserDefinedTarget = IsUserDefinedTarget;
            targetData.TracerouteGUID = TracerouteGUID;
            targetData.TargetGUID = TargetGUID;
            targetData.Hop = Hop;
            targetData.TargetStatus = Status;
            targetData.TimeStamp = await CalculateTimeStampAsync();

            return targetData;
        }

        public async Task<LatencyMonitorData> UpdateTargetDataAsync()
        {
            var targetData = new LatencyMonitorData();
            PingReply response;
            int rtt = 0;
            IPStatus ips = IPStatus.Unknown;

            if (Status == LatencyMonitorTargetStatus.Active)
            {
                using (var ping = new Ping())
                {
                    response = await ping.SendPingAsync(TargetAddress, 4000, new byte[32]);
                    rtt = (int)response.RoundtripTime;
                    ips = response.Status;
                }

                targetData.Latency = await CalculateLatencyAsync(rtt);
                targetData.LowestLatency = await CalculateLowestLatencyAsync(rtt, ips, Data);
                targetData.HighestLatency = await CalculateHighestLatencyAsync(rtt, ips, Data);
                targetData.AverageLatency = await CalculateAverageLatencyAsync(ips, Data);
                targetData.TotalPacketsLost = await CalculateTotalPacketsLostAsync(ips, Data);
                targetData.AverageLatencyCounter = await CalculateAverageLatencyCounterAsync(ips, Data);
                targetData.FailedPing = await CalculateFailedPingAsync(ips);
                targetData.TotalLatency = await CalculateTotalLatencyAsync(rtt, Data);
            }

            targetData.ReportID = ReportID;
            targetData.DisplayName = DisplayName;
            targetData.TargetName = TargetName;
            targetData.TargetAddress = TargetAddress;
            targetData.IsUserDefinedTarget = IsUserDefinedTarget;
            targetData.TracerouteGUID = TracerouteGUID;
            targetData.TargetGUID = TargetGUID;
            targetData.Hop = Hop;
            targetData.TargetStatus = Status;
            targetData.TimeStamp = await CalculateTimeStampAsync();

            return targetData;
        }
        #region Private Methods

        #endregion Private Methods
    }
}
