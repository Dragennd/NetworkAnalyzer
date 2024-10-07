using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.LatencyHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.PacketLossHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.TimeStampHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.Functions.StatusHandler;
using static NetworkAnalyzer.Apps.LatencyMonitor.LatencyMonitorManager;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class TracerouteHandler
    {
        private string TargetName { get; set; }
        private string? HopTargetName { get; set; }
        private string FailedPingName { get; set; } = "Request timed out";
        private IPStatus PingStatus { get; set; }
        private int RoundTripTime { get; set; }
        private int KeyPosition { get; set; } = 0;
        private int Hop { get; set; } = 1;
        private int FailedHopCount { get; set; } = 1;
        private int TotalHops { get; set; }
        private bool Initialization { get; set; }
        private LatencyMonitorSessionStatus LMSStatus { get; set; }
        private TracerouteStatus TRStatus { get; set; } = TracerouteStatus.Failed;

        public TracerouteHandler(string targetName, bool initialization, int totalHops, [Optional]IPStatus status, [Optional]long roundTripTime)
        {
            TargetName = targetName;
            PingStatus = status;
            RoundTripTime = Convert.ToInt32(roundTripTime);
            Initialization = initialization;
            TotalHops = totalHops;
        }

        public async Task<LatencyMonitorData> NewTracerouteSuccessDataAsync()
        {
            var data = new LatencyMonitorData();

            if (Initialization)
            {
                var dns = new DNSHandler();

                data.TargetName = HopTargetName;
                data.DNSHostName = await dns.GetDeviceNameAsync(HopTargetName);
                data.Latency = RoundTripTime;
                data.LowestLatency = await CalculateLowestLatencyAsync(PingStatus, RoundTripTime, HopTargetName, Initialization);
                data.HighestLatency = await CalculateHighestLatencyAsync(PingStatus, RoundTripTime, HopTargetName, Initialization);
                data.AverageLatency = await CalculateAverageLatencyAsync(PingStatus, RoundTripTime, HopTargetName, Initialization);
                data.AverageLatencyCounter = await CalculateAverageLatencyCounter(RoundTripTime, TargetName, Initialization);
                data.TotalLatency = await CalculateTotalLatencyAsync(RoundTripTime, HopTargetName, Initialization);
                data.TotalPacketsLost = await CalculateTotalPacketsLostAsync(PingStatus, HopTargetName, Initialization);
                data.FailedPing = await CalculateFailedPingAsync(PingStatus, HopTargetName, Initialization);
                data.TimeStamp = await CalculateTimeStampAsync();
                data.Hop = Hop;
                data.Status = LMSStatus;
            }
            else
            {
                var lastDataSet = LiveSessionData[TargetName].Last();
                await RemoveSessionDataAsync(TargetName);

                data.TargetName = TargetName;
                data.DNSHostName = lastDataSet.DNSHostName;
                data.Latency = RoundTripTime;
                data.LowestLatency = await CalculateLowestLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
                data.HighestLatency = await CalculateHighestLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
                data.AverageLatency = await CalculateAverageLatencyAsync(PingStatus, RoundTripTime, TargetName, Initialization);
                data.AverageLatencyCounter = await CalculateAverageLatencyCounter(RoundTripTime, TargetName, Initialization);
                data.TotalLatency = await CalculateTotalLatencyAsync(RoundTripTime, TargetName, Initialization);
                data.TotalPacketsLost = await CalculateTotalPacketsLostAsync(PingStatus, TargetName, Initialization);
                data.FailedPing = await CalculateFailedPingAsync(PingStatus, TargetName, Initialization);
                data.TimeStamp = await CalculateTimeStampAsync();
                data.Hop = await MaintainAssignedHopAsync();
                data.Status = await CalculateCurrentStatusAsync(PingStatus, TargetName, Initialization);
            }

            return data;
        }

        public async Task<LatencyMonitorData> NewTracerouteFailureDataAsync()
        {
            var data = new LatencyMonitorData();

            data.TargetName = FailedPingName;
            data.Hop = Hop;
            data.FailedHopCounter = FailedHopCount;

            FailedHopCount++;

            if (LMSStatus == LatencyMonitorSessionStatus.NoResponse)
            {
                data.Status = LMSStatus;
            }

            return await Task.FromResult(data);
        }

        public async Task<TracerouteStatus> PerformInitialTraceroute()
        {
            try
            {
                var dbHandler = new DatabaseHandler();
                var targetIP = Dns.GetHostAddresses(TargetName).First(address => address.AddressFamily == AddressFamily.InterNetwork).ToString();

                while (Hop <= TotalHops && TRStatus != TracerouteStatus.Unresolved)
                {
                    var (ipAddress, status, latency) = await GetNextHopDestinationAsync(targetIP);
                    RoundTripTime = latency;

                    // Check for favorable status and that hop be greater than 0 but less than 3
                    // If so, set status to Up
                    if ((status == IPStatus.Success || status == IPStatus.TtlExpired) && Hop < 3)
                    {
                        LMSStatus = LatencyMonitorSessionStatus.Up;
                        PingStatus = status;
                    }
                    // Check for favorable status and that hop be greater than or equal to 3 and that latency be greater than 0
                    // If so, set status to Up
                    else if ((status == IPStatus.Success || status == IPStatus.TtlExpired) && Hop >= 3 && latency > 0)
                    {
                        LMSStatus = LatencyMonitorSessionStatus.Up;
                        PingStatus = status;
                    }
                    else if (status == IPStatus.Unknown)
                    {
                        LMSStatus = LatencyMonitorSessionStatus.NoResponse;
                        FailedPingName = ipAddress;
                    }
                    else
                    {
                        LMSStatus = LatencyMonitorSessionStatus.Down;
                        PingStatus = status;
                    }

                    // Create the LatencyMonitorData object based on the target IP in the traceroute
                    if (ipAddress == targetIP && status != IPStatus.Unknown)
                    {
                        HopTargetName = ipAddress;
                        var targetData = await NewTracerouteSuccessDataAsync();

                        // Create a new dictionary entry for the final hop in the Traceroute
                        await CreateSessionAsync(HopTargetName, targetData);
                        await dbHandler.NewLatencyMonitorReportSnapshotAsync(targetData);
                        await dbHandler.NewLatencyMonitorReportEntryAsync(targetData);

                        // Add the final hop target to the IPAddresses list to continue the scan
                        IPAddresses.Add(ipAddress);

                        TRStatus = TracerouteStatus.Completed;
                        break;
                    }
                    else if (ipAddress != targetIP && ipAddress != "0.0.0.0" && status != IPStatus.Unknown)
                    {
                        HopTargetName = ipAddress;
                        var targetData = await NewTracerouteSuccessDataAsync();

                        // Create a new dictionary entry for the specified hop in the Traceroute
                        await CreateSessionAsync(HopTargetName, targetData);
                        await dbHandler.NewLatencyMonitorReportSnapshotAsync(targetData);
                        await dbHandler.NewLatencyMonitorReportEntryAsync(targetData);

                        // Add the specified hop target to the IPAddresses list to continue the scan
                        IPAddresses.Add(ipAddress);

                        TRStatus = TracerouteStatus.Failed;
                    }
                    else
                    {
                        var targetData = await NewTracerouteFailureDataAsync();

                        // Create a new dictionary entry for the specified hop in the Traceroute which failed to return a response
                        await CreateSessionAsync(await GenerateNoResponseKeyAsync(), targetData);
                        await dbHandler.NewLatencyMonitorReportSnapshotAsync(targetData);

                        TRStatus = TracerouteStatus.Failed;
                    }

                    Hop++;
                }
            }
            catch (SocketException)
            {
                TRStatus = TracerouteStatus.Unresolved;
            }

            return TRStatus;
        }

        // Copy the number which was assigned to the target from round to round so it maintains its order
        private async Task<int> MaintainAssignedHopAsync()
        {
            var lastDataSet = LiveSessionData[TargetName].LastOrDefault();

            if (lastDataSet.Hop >= 1)
            {
                Hop = lastDataSet.Hop;
            }
            else
            {
                Hop = -1;
            }

            return await Task.FromResult(Hop);
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
        private async Task<(string ipAddress, IPStatus status, int latency)> GetNextHopDestinationAsync(string targetName)
        {
            IPStatus tempStatus = IPStatus.Unknown;
            int failedPingCount = 0;
            int successfulPingCount = 0;

            var options = new PingOptions()
            {
                Ttl = Hop,
                DontFragment = true,
            };

            var sw = Stopwatch.StartNew();
            var response = await new Ping().SendPingAsync(targetName, 4000, new byte[32], options);
            sw.Stop();

            if (response.Address.ToString() != "0.0.0.0")
            {
                do
                {
                    var secondResponse = await new Ping().SendPingAsync(response.Address, 1000);
                    if (secondResponse.Status != IPStatus.Success)
                    {
                        failedPingCount++;
                    }
                    else if (secondResponse.Status == IPStatus.Success && successfulPingCount < 3)
                    {
                        successfulPingCount++;
                    }
                    else
                    {
                        break;
                    }
                } while (failedPingCount <= 4);
            }

            if (response.Address.ToString() == "0.0.0.0")
            {
                tempStatus = response.Status;
                FailedPingName = "Request timed out";
            }
            else if (failedPingCount < 3)
            {
                tempStatus = IPStatus.Success;
            }

            return (response.Address.ToString(), tempStatus, (int)sw.Elapsed.TotalMilliseconds);
        }
    }
}
