using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public class LatencyMonitorFunction
    {
        // Used to control the main flow of the monitoring session
        public static async Task ProcessDataAsync(string ipAddress)
        {
            var pingResults = await new Ping().SendPingAsync(ipAddress, 1000);
            var lastDataSet = LiveData[ipAddress].LastOrDefault();

            LiveData[ipAddress]
            .Add(new LatencyMonitorData()
            {
                IPAddress = ipAddress,
                Status = pingResults.Status,
                Latency = (int)pingResults.RoundtripTime,
                LowestLatency = await LatencyHandler.CalculateLowestLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                HighestLatency = await LatencyHandler.CalculateHighestLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                AverageLatencyDivider = await LatencyHandler.CalculateCounterAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                AllAveragesCombined = lastDataSet.AllAveragesCombined + (int)pingResults.RoundtripTime,
                AverageLatency = await LatencyHandler.CalculateAverageLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                PacketsLostTotal = await PacketLossHandler.CalculatePacketsLostTotalAsync(pingResults.Status, ipAddress),
                FailedPings = await PacketLossHandler.CalculateFailedPingsAsync(pingResults.Status, ipAddress),
                ConnectionStatus = await StatusHandler.CalculateCurrentStatusAsync(pingResults.Status, ipAddress),
                TimeStampOfLastMajorChange = await TimeStampHandler.CalculateLastMajorChangeAsync(ipAddress, await StatusHandler.CalculateCurrentStatusAsync(pingResults.Status, ipAddress))
            });
        }

        // Used at the start of the session to assign the initial data to the LiveData and ReportData dictionaries
        public static async Task InitializeDataAsync(string ipAddress)
        {
            var pingResults = await new Ping().SendPingAsync(ipAddress, 1000);

            LiveData
            .Add(ipAddress, new List<LatencyMonitorData>
            { new()
                {
                    IPAddress = ipAddress,
                    Status = pingResults.Status,
                    Latency = (int)pingResults.RoundtripTime,
                    AverageLatencyDivider = await LatencyHandler.CalculateCounterAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                    LowestLatency = (int)pingResults.RoundtripTime,
                    HighestLatency = (int)pingResults.RoundtripTime,
                    AverageLatency = (int)pingResults.RoundtripTime,
                    AllAveragesCombined = (int)pingResults.RoundtripTime,
                    PacketsLostTotal = await PacketLossHandler.CalculatePacketsLostTotalAsync(pingResults.Status, ipAddress),
                    FailedPings = await PacketLossHandler.CalculateFailedPingsAsync(pingResults.Status, ipAddress),
                    ConnectionStatus = await StatusHandler.CalculateCurrentStatusAsync(pingResults.Status, ipAddress),
                    TimeStampOfLastMajorChange = DateTime.Now
                }
            });

            var lastDataSet = LiveData[ipAddress].LastOrDefault();

            ReportData
            .Add(ipAddress, new List<LatencyMonitorData>
            { new()
                {
                    IPAddress = ipAddress,
                    Status = lastDataSet.Status,
                    Latency = lastDataSet.Latency,
                    LowestLatency = lastDataSet.LowestLatency,
                    HighestLatency = lastDataSet.HighestLatency,
                    AverageLatency = lastDataSet.AverageLatency,
                    PacketsLostTotal = lastDataSet.PacketsLostTotal,
                    FailedPings = lastDataSet.FailedPings,
                    ConnectionStatus = lastDataSet.ConnectionStatus,
                    TimeStampOfLastMajorChange = DateTime.Now
                }
            });
        }

        // Used generically to add data to the ReportData dictionary
        public static void WriteToReportData(string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();

            ReportData[ipAddress]
            .Add(new LatencyMonitorData()
            {
                IPAddress = ipAddress,
                Status = lastDataSet.Status,
                Latency = lastDataSet.Latency,
                LowestLatency = lastDataSet.LowestLatency,
                HighestLatency = lastDataSet.HighestLatency,
                AverageLatency = lastDataSet.AverageLatency,
                PacketsLostTotal = lastDataSet.PacketsLostTotal,
                FailedPings = lastDataSet.FailedPings,
                ConnectionStatus = lastDataSet.ConnectionStatus,
                TimeStampOfLastMajorChange = DateTime.Now
            });
        }
    }
}
