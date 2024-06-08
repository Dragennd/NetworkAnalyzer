using System.Net.NetworkInformation;
using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public class LatencyMonitorFunction
    {
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
                LowestLatency = await CalculateLowestLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                HighestLatency = await CalculateHighestLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                AverageLatencyDivider = await CalculateCounterAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                AllAveragesCombined = lastDataSet.AllAveragesCombined + (int)pingResults.RoundtripTime,
                AverageLatency = await CalculateAverageLatencyAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                PacketsLostTotal = await CalculatePacketsLostTotalAsync(pingResults.Status, ipAddress),
                FailedPings = await CalculateFailedPingsAsync(pingResults.Status, ipAddress),
                ConnectionStatus = await CalculateCurrentStatusAsync(pingResults.Status, ipAddress),
                TimeStampOfLastMajorChange = await CalculateLastMajorChangeAsync(ipAddress, await CalculateCurrentStatusAsync(pingResults.Status, ipAddress))
            });
        }

        public static async Task<string> CalculateCurrentStatusAsync(IPStatus status, string ipAddress)
        {
            string responseCode;

            if (!LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    responseCode = "Down";
                }
                else
                {
                    responseCode = "Up";
                }
            }
            else
            {
                var dataSet = LiveData[ipAddress];

                if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) > 0.125
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) < .50)
                {
                    responseCode = "Unstable";
                    LatencyMonitorReport.ProcessLastMajorChangeAsync(ipAddress, responseCode);
                }
                else if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) >= .50)
                {
                    responseCode = "Down";
                    LatencyMonitorReport.ProcessLastMajorChangeAsync(ipAddress, responseCode);
                }
                else
                {
                    responseCode = "Up";
                    LatencyMonitorReport.ProcessLastMajorChangeAsync(ipAddress, responseCode);
                }
            }

            return await Task.FromResult(responseCode);
        }

        public static async Task<int> CalculateLowestLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int lowestLatency = 0;

            if (status == IPStatus.Success && latency <= lastDataSet.LowestLatency)
            {
                lowestLatency = latency;
            }
            else
            {
                lowestLatency = lastDataSet.LowestLatency;
            }

            return await Task.FromResult(lowestLatency);
        }

        public static async Task<int> CalculateHighestLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int highestLatency = 0;

            if (status == IPStatus.Success && latency >= lastDataSet.HighestLatency)
            {
                highestLatency = latency;
            }
            else
            {
                highestLatency = lastDataSet.HighestLatency;
            }

            return await Task.FromResult(highestLatency);
        }

        public static async Task<int> CalculateAverageLatencyAsync(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();
            int averageLatency = 0;

            if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
            {
                averageLatency = lastDataSet.AllAveragesCombined / lastDataSet.AverageLatencyDivider;
            }
            else if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency == 0)
            {
                averageLatency = latency;
            }
            else
            {
                averageLatency = lastDataSet.AverageLatency;
            }

            return await Task.FromResult(averageLatency);
        }

        public static async Task<int> CalculatePacketsLostTotalAsync(IPStatus status, string ipAddress)
        {
            int packetsLost = 0;

            if (!LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    packetsLost = 1;
                }
                else
                {
                    packetsLost = 0;
                }
            }
            else
            {
                var lastDataSet = LiveData[ipAddress].LastOrDefault();

                if (status != IPStatus.Success)
                {
                    packetsLost = lastDataSet.PacketsLostTotal + 1;
                }
                else
                {
                    packetsLost = lastDataSet.PacketsLostTotal;
                }
            }

            return await Task.FromResult(packetsLost);
        }

        public static async Task<int> CalculateFailedPingsAsync(IPStatus status, string ipAddress)
        {
            int failedPings = 0;

            if (!LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    failedPings = 1;
                }
                else
                {
                    failedPings = 0;
                }
            }
            else
            {
                var dataSet = LiveData[ipAddress];
                bool currentStatusCheck = status == IPStatus.Success;
                bool firstStatusCheck = dataSet.FirstOrDefault().Status == IPStatus.Success;
                bool maxedOutLiveData = dataSet.Count == 60;

                if ((!currentStatusCheck) && (!firstStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    failedPings = dataSet.LastOrDefault().FailedPings;
                }
                else if (currentStatusCheck && (!firstStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    failedPings = dataSet.LastOrDefault().FailedPings - 1;
                }
                else if ((!currentStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    failedPings = dataSet.LastOrDefault().FailedPings + 1;
                }
                else if (maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    failedPings = dataSet.LastOrDefault().FailedPings;
                }
                else if (!currentStatusCheck)
                {
                    failedPings = dataSet.LastOrDefault().FailedPings + 1;
                }
                else
                {
                    failedPings = dataSet.LastOrDefault().FailedPings;
                }
            }

            return await Task.FromResult(failedPings);
        }

        public static async Task<int> CalculateCounterAsync(IPStatus status, long latency, string ipAddress)
        {
            int averageLatencyDivider = 0;

            if (!LiveData.ContainsKey(ipAddress))
            {
                averageLatencyDivider = 1;
            }
            else
            {
                var lastDataSet = LiveData[ipAddress].LastOrDefault();

                if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
                {
                    averageLatencyDivider = lastDataSet.AverageLatencyDivider + 1;
                }
                else
                {
                    averageLatencyDivider = lastDataSet.AverageLatencyDivider;
                }
            }

            return await Task.FromResult(averageLatencyDivider);
        }

        public static async Task<DateTime> CalculateLastMajorChangeAsync(string ipAddress, string responseCode)
        {
            var lastDataSet = LiveData[ipAddress].LastOrDefault();

            if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Down"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently down, was previously down and its been half an hour
                // Updating the dictionary if the internet is still down and its been half an hour
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Unstable"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently unstable, was previously unstable and its been half an hour
                // Updating the dictionary if the internet is still unstable and its been half an hour
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the connection was down but is slowly becoming better
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the connection was unstable and is now down completely
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet just went down and has been good
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet started being bad and has been good
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the internet was down but is now good
                return await Task.FromResult(DateTime.Now);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the internet was unstable but is now good
                return await Task.FromResult(DateTime.Now);
            }
            else
            {
                return await Task.FromResult(lastDataSet.TimeStampOfLastMajorChange);
            }
        }

        public static async Task InitializeData(string ipAddress)
        {
            var pingResults = await new Ping().SendPingAsync(ipAddress, 1000);

            LiveData
            .Add(ipAddress, new List<LatencyMonitorData>
            { new()
                {
                    IPAddress = ipAddress,
                    Status = pingResults.Status,
                    Latency = (int)pingResults.RoundtripTime,
                    AverageLatencyDivider = await CalculateCounterAsync(pingResults.Status, (int)pingResults.RoundtripTime, ipAddress),
                    LowestLatency = (int)pingResults.RoundtripTime,
                    HighestLatency = (int)pingResults.RoundtripTime,
                    AverageLatency = (int)pingResults.RoundtripTime,
                    AllAveragesCombined = (int)pingResults.RoundtripTime,
                    PacketsLostTotal = await CalculatePacketsLostTotalAsync(pingResults.Status, ipAddress),
                    FailedPings = await CalculateFailedPingsAsync(pingResults.Status, ipAddress),
                    ConnectionStatus = await CalculateCurrentStatusAsync(pingResults.Status, ipAddress),
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
    }
}
