using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class UserTargetsHandler
    {
        private string TargetName { get; set; }
        private IPStatus PingStatus { get; set; }
        private int RoundTripTime { get; set; }
        private bool Initialization { get; set; }

        public UserTargetsHandler(string targetName, IPStatus status, long roundTripTime, bool initialization)
        {
            TargetName = targetName;
            PingStatus = status;
            RoundTripTime = Convert.ToInt32(roundTripTime);
            Initialization = initialization;
        }

        public async Task<LatencyMonitorData> NewUserTargetsDataAsync()
        {
            var data = new LatencyMonitorData();

            if (!Initialization)
            {
                await RemoveSessionDataAsync(TargetName);
            }

            data.TargetName = TargetName;
            data.Status = await CalculateCurrentStatusAsync(PingStatus, TargetName, Initialization);
            data.Latency = RoundTripTime;
            data.LowestLatency = await CalculateLowestLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
            data.HighestLatency = await CalculateHighestLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
            data.AverageLatency = await CalculateAverageLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
            data.TotalLatency = await CalculateTotalLatencyAsync(RoundTripTime, TargetName, Initialization);
            data.TotalPacketsLost = await CalculateTotalPacketsLostAsync(PingStatus, TargetName, Initialization);
            data.FailedPing = await CalculateFailedPingAsync(PingStatus, TargetName, Initialization);
            data.TimeStamp = await CalculateTimeStampAsync();

            return data;
        }
    }
}
