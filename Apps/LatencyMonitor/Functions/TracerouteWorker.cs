using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteWorker
    {
        private string DisplayName { get; set; }
        private string TargetName { get; set; }
        private string TargetAddress { get; set; }
        private string TracerouteGUID { get; set; }
        private string CurrentTarget { get; set; }
        private int Hop { get; set; } = 1;
        private List<LatencyMonitorData> TargetData { get; set; }
        private readonly ILatencyMonitorController _latencyMonitorController;

        public TracerouteWorker(string targetName, ILatencyMonitorController latencyMonitorController)
        {
            _latencyMonitorController = latencyMonitorController;
            DisplayName = targetName;
            TracerouteGUID = Guid.NewGuid().ToString();
            TargetData = new();
        }

        public async Task<List<LatencyMonitorData>> NewTracerouteDataAsync()
        {
            await SetTargetsAsync();

            var z = new TargetWorker(
                displayName: DisplayName,
                targetName: TargetName,
                targetAddress: TargetAddress,
                tracerouteGUID: TracerouteGUID,
                isUserDefinedTarget: true,
                status: LatencyMonitorTargetStatus.Active
                );

            var data = await z.NewTargetDataAsync();

            TargetData.Add(data);

            _latencyMonitorController.SendSetLiveTargetRequest(data);
            _latencyMonitorController.SendSetSelectedTargetRequest(data);

            do
            {
                var hopData = await GetNextHopDataAsync();
                CurrentTarget = hopData.Item1;
                if (CurrentTarget == TargetAddress)
                {
                    var v = TargetData.First(a => a.IsUserDefinedTarget == true);
                    v.Hop = Hop;
                }
                else
                {
                    var u = new TargetWorker(
                        displayName: CurrentTarget,
                        targetName: hopData.Item3,
                        targetAddress: CurrentTarget,
                        tracerouteGUID: TracerouteGUID,
                        isUserDefinedTarget: false,
                        hop: Hop,
                        status: hopData.Item2
                        );

                    var x = await u.NewTargetDataAsync();

                    TargetData.Add(x);

                    _latencyMonitorController.SendSetTracerouteRequest(x);
                }

                Hop++;
            } while (CurrentTarget != TargetAddress);

            _latencyMonitorController.SendSetTracerouteRequest(data);

            return TargetData;
        }

        #region Private Methods
        private async Task<(string, LatencyMonitorTargetStatus, string)> GetNextHopDataAsync()
        {
            PingReply response;
            string target;
            string name;
            LatencyMonitorTargetStatus status;

            var options = new PingOptions()
            {
                Ttl = Hop,
                DontFragment = true,
            };

            using (var ping = new Ping())
            {
                response = await ping.SendPingAsync(TargetAddress, 4000, new byte[32], options);
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
                try
                {
                    response = await ping.SendPingAsync(target, 4000, new byte[32]);
                }
                catch (ArgumentException)
                {
                    status = LatencyMonitorTargetStatus.NoResponse;
                }
                catch (PingException)
                {
                    status = LatencyMonitorTargetStatus.NoResponse;
                }
            }

            if (response.Status == IPStatus.Success)
            {
                status = LatencyMonitorTargetStatus.Active;
                name = await DNSHandler.GetDeviceNameAsync(target);
            }
            else if (response.Status != IPStatus.Success && target != "Request timed out")
            {
                status = LatencyMonitorTargetStatus.Inactive;
                name = await DNSHandler.GetDeviceNameAsync(target);
            }
            else if (response.Status != IPStatus.Success && target == "Request timed out")
            {
                status = LatencyMonitorTargetStatus.NoResponse;
                name = "Request timed out";
            }
            else
            {
                status = LatencyMonitorTargetStatus.None;
                name = "Request timed out";
            }

            return (target, status, name);
        }

        private async Task SetTargetsAsync()
        {
            TargetName = await DNSHandler.GetDeviceNameAsync(DisplayName);
            TargetAddress = await DNSHandler.ResolveIPAddressFromDNSAsync(DisplayName);
        }
        #endregion Private Methods
    }
}
