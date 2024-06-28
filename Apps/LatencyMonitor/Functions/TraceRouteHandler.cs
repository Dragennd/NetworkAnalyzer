using System.Net.NetworkInformation;
using System.Text;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TraceRouteHandler
    {
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

                if (response.Status == IPStatus.Success || response.Status == IPStatus.TtlExpired)
                {
                    status = LatencyMonitorSessionStatus.Up;
                }
                else
                {
                    status = LatencyMonitorSessionStatus.Down;
                }

                if (response.Address.ToString() == targetName && response.Address.ToString() != "0.0.0.0")
                {
                    // Create a new dictionary entry for the final hop in the traceroute
                    await manager.CreateSessionAsync(response.Address.ToString(),
                                         await manager.NewSessionDataAsync(response.Address.ToString(), (int)response.RoundtripTime, status,
                                                       await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, response.Address.ToString(), true),
                                                       await packetLossHandler.CalculateFailedPingAsync(response.Status, response.Address.ToString(), true),
                                                       await timeStampHandler.CalculateTimeStampAsync(response.Address.ToString(), status, LatencyMonitorSessionType.Live, true),
                                                       hop));

                    // Add the final hop target to the IPAddresses list to continue the scan
                    IPAddresses.Add(response.Address.ToString());

                    break;
                }
                if (response.Address.ToString() != targetName && response.Address.ToString() != "0.0.0.0")
                {
                    // Create a new dictionary entry for the specified hop in the traceroute
                    await manager.CreateSessionAsync(response.Address.ToString(),
                                         await manager.NewSessionDataAsync(response.Address.ToString(), (int)response.RoundtripTime, status,
                                                       await latencyHandler.CalculateLowestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateHighestLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateAverageLatencyAsync(response.Status, (int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await latencyHandler.CalculateTotalLatencyAsync((int)response.RoundtripTime, response.Address.ToString(), true),
                                                       await packetLossHandler.CalculateTotalPacketsLostAsync(response.Status, response.Address.ToString(), true),
                                                       await packetLossHandler.CalculateFailedPingAsync(response.Status, response.Address.ToString(), true),
                                                       await timeStampHandler.CalculateTimeStampAsync(response.Address.ToString(), status, LatencyMonitorSessionType.Live, true),
                                                       hop));

                    // Add the specified hop target to the IPAddresses list to continue the scan
                    IPAddresses.Add(response.Address.ToString());
                }

                hop++;
            }
        }

        public async Task<int> MaintainAssignedHop(string targetName)
        {
            LatencyMonitorData lastDataSet = LiveSessionData[targetName].LastOrDefault();

            if (lastDataSet.Hop != null)
            {
                return lastDataSet.Hop;
            }
            else
            {
                return -1;
            }
        }

        private async Task<PingReply> GetNextHopDestinationAsync(string targetName, int hop)
        {
            PingOptions options = new()
            {
                Ttl = hop
            };

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 2000;

            return await new Ping().SendPingAsync(targetName, timeout, buffer, options);
        }
    }
}
