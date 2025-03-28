using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        private string TargetName { get; set; }
        private string CurrentTarget { get; set; } = string.Empty;
        private int Hop { get; set; } = 1;
        private List<LatencyMonitorData> TargetData { get; set; }

        public TracerouteHandler(string targetName)
        {
            TargetName = targetName;
            TargetData = new();
        }

        public async Task<List<LatencyMonitorData>> NewTracerouteDataAsync()
        {
            do
            {
                var hopData = await GetNextHopDataAsync();
                CurrentTarget = hopData.Item1;
                var u = new TargetHandler(targetName: TargetName, userDefinedTarget: CurrentTarget, hop: Hop, status: hopData.Item2);

                TargetData.Add(await u.NewTargetDataAsync());
                Hop++;
            } while (TargetName != CurrentTarget);

            return TargetData;
        }

        #region Private Methods
        private async Task<(string, LatencyMonitorTargetStatus)> GetNextHopDataAsync()
        {
            PingReply response;
            string target;
            LatencyMonitorTargetStatus status;

            var options = new PingOptions()
            {
                Ttl = Hop,
                DontFragment = true,
            };

            using (var ping = new Ping())
            {
                response = await ping.SendPingAsync(TargetName, 4000, new byte[32], options);
            }

            if (response.Address.ToString() == "0.0.0.0")
            {
                target = "Request timed out";
            }
            else
            {
                target = response.Address.ToString();
            }

            using (var ping = new Ping())
            {
                response = await ping.SendPingAsync(target, 4000, new byte[32]);
            }

            if (response.Status == IPStatus.Success)
            {
                status = LatencyMonitorTargetStatus.Active;
            }
            else if (response.Status != IPStatus.Success && target != "Request timed out")
            {
                status = LatencyMonitorTargetStatus.Inactive;
            }
            else if (response.Status != IPStatus.Success && target == "Request timed out")
            {
                status = LatencyMonitorTargetStatus.NoResponse;
            }
            else
            {
                status = LatencyMonitorTargetStatus.None;
            }

                return (target, status);
        }
        #endregion Private Methods
    }
}
