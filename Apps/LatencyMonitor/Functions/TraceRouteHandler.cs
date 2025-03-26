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
                CurrentTarget = await GetNextHopDataAsync();
                var u = new UserTargetsHandler(TargetName, CurrentTarget, Hop);

                TargetData.Add(await u.NewUserTargetDataAsync());
                Hop++;
            } while (TargetName != CurrentTarget);

            return TargetData;
        }

        #region Private Methods
        private async Task<string> GetNextHopDataAsync()
        {
            PingReply response;

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
                return "Request timed out";
            }
            else
            {
                return response.Address.ToString();
            }
        }
        #endregion Private Methods
    }
}
