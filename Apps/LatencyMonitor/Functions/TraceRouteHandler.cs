using System.Net.NetworkInformation;
using System.Text;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        public int KeyPosition { get; set; } = 0;

        public async Task ProcessHopsAsync(string targetName, int totalHops)
        {
            LatencyMonitorManager manager = new();
            LatencyHandler latencyHandler = new();
            StatusHandler statusHandler = new();
            PacketLossHandler packetLossHandler = new();
            TimeStampHandler timeStampHandler = new();

            int hop = 1;
            LatencyMonitorSessionStatus status;

            while (hop <= totalHops)
            {
                var response = await GetNextHopDestinationAsync(targetName, hop);

                // Check for favorable status and that hop be greater than 0 but less than 3
                // If so, set status to Up
                if ((response.Status == IPStatus.Success || response.Status == IPStatus.TtlExpired) && hop < 3)
                {
                    status = LatencyMonitorSessionStatus.Up;
                }
                // Check for favorable status and that hop be greater than or equal to 3 and that latency be greater than 0
                // If so, set status to Up
                else if ((response.Status == IPStatus.Success || response.Status == IPStatus.TtlExpired) && hop >= 3 && (int)response.RoundtripTime > 0)
                {
                    status = LatencyMonitorSessionStatus.Up;
                }
                else
                {
                    status = LatencyMonitorSessionStatus.Down;
                }

                if (response.Address.ToString() == targetName && response.Address.ToString() != "0.0.0.0")
                {
                    // Create a new dictionary entry for the final hop in the Traceroute
                    await manager.CreateSessionAsync(response.Address.ToString(),
                        await manager.NewSessionDataAsync(response.Address.ToString(), (int)response.RoundtripTime, status,
                        await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                        await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                        await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                        await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, response.Address.ToString(), true),
                        await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, response.Address.ToString(), true),
                        await packetLossHandler.CalculateFailedPingAsync(response.Status, response.Address.ToString(), true),
                        await timeStampHandler.CalculateTimeStampAsync(),
                        hop));

                    // Add the final hop target to the IPAddresses list to continue the scan
                    IPAddresses.Add(response.Address.ToString());

                    break;
                }
                else if (response.Address.ToString() != targetName && response.Address.ToString() != "0.0.0.0")
                {
                    // Create a new dictionary entry for the specified hop in the Traceroute
                    await manager.CreateSessionAsync(response.Address.ToString(),
                        await manager.NewSessionDataAsync(response.Address.ToString(), (int)response.RoundtripTime, status,
                            await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                            await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                            await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                            await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, response.Address.ToString(), true),
                            await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, response.Address.ToString(), true),
                            await packetLossHandler.CalculateFailedPingAsync(response.Status, response.Address.ToString(), true),
                            await timeStampHandler.CalculateTimeStampAsync(),
                            hop));

                    // Add the specified hop target to the IPAddresses list to continue the scan
                    IPAddresses.Add(response.Address.ToString());
                }
                else
                {
                    await manager.CreateSessionAsync(await GenerateNoResponseKeyAsync(),
                        await manager.NewSessionDataAsync("Request timed out", hop));
                }

                hop++;
            }
        }

        public async Task<int> MaintainAssignedHopAsync(string targetName)
        {
            LatencyMonitorData lastDataSet = LiveSessionData[targetName].LastOrDefault();

            int hop = 0;

            if (lastDataSet.Hop != null)
            {
                hop = lastDataSet.Hop;
            }
            else
            {
                hop = -1;
            }

            return await Task.FromResult(hop);
        }

        private async Task<string> GenerateNoResponseKeyAsync()
        {
            var keys = LiveSessionData.Keys;
            string responseKey = string.Empty;

            do
            {
                responseKey = "NoResponse." + KeyPosition;

                if (keys.Contains(responseKey))
                {
                    KeyPosition++;
                }
            } while (keys.Contains(responseKey));

            return await Task.FromResult(responseKey);
        }

        private async Task<PingReply> GetNextHopDestinationAsync(string targetName, int hop)
        {
            PingOptions options = new()
            {
                Ttl = hop
            };

            var response =  await new Ping().SendPingAsync(targetName, 5000, new byte[32], options);
            if (response.Address.ToString() == "0.0.0.0")
            {
                return await Task.FromResult(response);
            }
            else
            {
                return await GetNextHopICMPDataAsync(response.Address.ToString());
            }
        }

        private async Task<PingReply> GetNextHopICMPDataAsync(string targetName)
        {
            return await new Ping().SendPingAsync(targetName, 5000);
        }
    }
}
