using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        public int KeyPosition { get; set; } = 0;

        public async Task<TracerouteStatus> ProcessHopsAsync(string targetName, int totalHops)
        {
            LatencyMonitorManager manager = new();
            LatencyHandler latencyHandler = new();
            StatusHandler statusHandler = new();
            PacketLossHandler packetLossHandler = new();
            TimeStampHandler timeStampHandler = new();

            TracerouteStatus trStatus = TracerouteStatus.Failed;

            try
            {
                int hop = 1;
                //IPHostEntry targetHostEntry = await Dns.GetHostEntryAsync(targetName);
                //string targetIP = targetHostEntry.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork).ToString();
                var targetIP = Dns.GetHostAddresses(targetName).First(address => address.AddressFamily == AddressFamily.InterNetwork).ToString();
                LatencyMonitorSessionStatus lmsStatus;

                while (hop <= totalHops && trStatus != TracerouteStatus.Unresolved)
                {
                    var response = await GetNextHopDestinationAsync(targetIP, hop);

                    // Check for favorable status and that hop be greater than 0 but less than 3
                    // If so, set status to Up
                    if ((response.status == IPStatus.Success || response.status == IPStatus.TtlExpired) && hop < 3)
                    {
                        lmsStatus = LatencyMonitorSessionStatus.Up;
                    }
                    // Check for favorable status and that hop be greater than or equal to 3 and that latency be greater than 0
                    // If so, set status to Up
                    else if ((response.status == IPStatus.Success || response.status == IPStatus.TtlExpired) && hop >= 3 && response.latency > 0)
                    {
                        lmsStatus = LatencyMonitorSessionStatus.Up;
                    }
                    else
                    {
                        lmsStatus = LatencyMonitorSessionStatus.Down;
                    }

                    if (response.ipAddress.ToString() == targetIP)
                    {
                        // Create a new dictionary entry for the final hop in the Traceroute
                        await manager.CreateSessionAsync(response.ipAddress,
                            await manager.NewSessionDataAsync(response.ipAddress, response.latency, lmsStatus,
                                await latencyHandler.CalculateLowestLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateHighestLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateAverageLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateTotalLatencyAsync(response.latency, response.ipAddress, true),
                                await packetLossHandler.CalculateTotalPacketsLostAsync(response.status, response.ipAddress, true),
                                await packetLossHandler.CalculateFailedPingAsync(response.status, response.ipAddress, true),
                                await timeStampHandler.CalculateTimeStampAsync(),
                                hop));

                        // Add the final hop target to the IPAddresses list to continue the scan
                        IPAddresses.Add(response.ipAddress);

                        trStatus = TracerouteStatus.Completed;
                        break;
                    }
                    else if (response.ipAddress.ToString() != targetIP && response.ipAddress.ToString() != "0.0.0.0")
                    {
                        // Create a new dictionary entry for the specified hop in the Traceroute
                        await manager.CreateSessionAsync(response.ipAddress,
                            await manager.NewSessionDataAsync(response.ipAddress, response.latency, lmsStatus,
                                await latencyHandler.CalculateLowestLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateHighestLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateAverageLatencyAsync(response.status, response.latency, response.ipAddress, true),
                                await latencyHandler.CalculateTotalLatencyAsync(response.latency, response.ipAddress, true),
                                await packetLossHandler.CalculateTotalPacketsLostAsync(response.status, response.ipAddress, true),
                                await packetLossHandler.CalculateFailedPingAsync(response.status, response.ipAddress, true),
                                await timeStampHandler.CalculateTimeStampAsync(),
                                hop));

                        // Add the specified hop target to the IPAddresses list to continue the scan
                        IPAddresses.Add(response.ipAddress);

                        trStatus = TracerouteStatus.Failed;
                    }
                    else
                    {
                        await manager.CreateSessionAsync(await GenerateNoResponseKeyAsync(),
                            await manager.NewSessionDataAsync("Request timed out", hop));

                        trStatus = TracerouteStatus.Failed;
                    }

                    hop++;
                }
            }
            catch (SocketException)
            {
                trStatus = TracerouteStatus.Unresolved;
            }

            return trStatus;
        }

        // Copy the number which was assigned to the target from round to round so it maintains its order
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

        // Create a unique key to use if the target found for a specified hop fails to send a ping reply
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

        // Ping the target with an ever-increasing TTL and return the data from that hop to the Traceroute
        private async Task<(string ipAddress, IPStatus status, int latency)> GetNextHopDestinationAsync(string targetName, int hop)
        {
            //var target = Dns.GetHostAddresses(targetName).First(address => address.AddressFamily == AddressFamily.InterNetwork);

            PingOptions options = new()
            {
                Ttl = hop,
                DontFragment = true,
            };

            Stopwatch sw = Stopwatch.StartNew();
            var response = await new Ping().SendPingAsync(targetName, 4000, new byte[32], options);
            sw.Stop();

            return (response.Address.ToString(), response.Status, (int)sw.Elapsed.TotalMilliseconds);
        }
    }
}
