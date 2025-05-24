using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        private string TargetName { get; set; } = string.Empty;
        private string CurrentTarget { get; set; } = string.Empty;
        private string FriendlyName { get; set; } = string.Empty;
        private string DNSHostName { get; set; } = string.Empty;
        private int Hop { get; set; } = 1;
        private List<LatencyMonitorData> TargetData { get; set; }
        private const string IPAddressFormat = @"\b(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))\b";

        public TracerouteHandler(string targetName)
        {
            FriendlyName = targetName;
            TargetData = new();
        }

        public async Task<List<LatencyMonitorData>> NewTracerouteDataAsync()
        {
            await SetTargetsAsync();

            do
            {
                var hopData = await GetNextHopDataAsync();
                CurrentTarget = hopData.Item1;
                var u = new TargetHandler(targetName: CurrentTarget,
                                          userDefinedTarget: DNSHostName,
                                          friendlyName: FriendlyName,
                                          dnsHostName: hopData.Item3,
                                          init: true,
                                          hop: Hop,
                                          status: hopData.Item2);

                TargetData.Add(await u.NewTargetDataAsync());
                Hop++;
            } while (CurrentTarget != DNSHostName);

            return TargetData;
        }

        #region Private Methods
        private async Task<(string, LatencyMonitorTargetStatus, string)> GetNextHopDataAsync()
        {
            PingReply response;
            string target;
            string hostName;
            LatencyMonitorTargetStatus status;

            var options = new PingOptions()
            {
                Ttl = Hop,
                DontFragment = true,
            };

            using (var ping = new Ping())
            {
                response = await ping.SendPingAsync(DNSHostName, 4000, new byte[32], options);
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
                hostName = await DNSHandler.ResolveIPAddressFromDNSAsync(target);
            }
            else if (response.Status != IPStatus.Success && target != "Request timed out")
            {
                status = LatencyMonitorTargetStatus.Inactive;
                hostName = "N/A";
            }
            else if (response.Status != IPStatus.Success && target == "Request timed out")
            {
                status = LatencyMonitorTargetStatus.NoResponse;
                hostName = "N/A";
            }
            else
            {
                status = LatencyMonitorTargetStatus.None;
                hostName = "N/A";
            }

            return (target, status, hostName);
        }

        private async Task SetTargetsAsync()
        {
            if (!Regex.IsMatch(FriendlyName, IPAddressFormat))
            {
                DNSHostName = await DNSHandler.ResolveIPAddressFromDNSAsync(FriendlyName);
                TargetName = FriendlyName;
            }
            else
            {
                DNSHostName = FriendlyName;
                TargetName = await DNSHandler.GetDeviceNameAsync(FriendlyName);
            }
            
        }
        #endregion Private Methods
    }
}
