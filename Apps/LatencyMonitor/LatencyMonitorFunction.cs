using NetworkAnalyzer.Apps.GlobalClasses;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public class LatencyMonitorFunction
    {
        public static void ProcessData(string ipAddress)
        {
            var pingResults = NetworkPing.PingTest(ipAddress);
            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            LatencyMonitorDataStorage
                .LiveData[ipAddress]
                .Add(new LatencyMonitorData()
                {
                    IPAddress = ipAddress,
                    Status = pingResults.Status,
                    Latency = pingResults.Latency,
                    AverageLatencyDivider = CalculateCounter(pingResults.Status, pingResults.Latency, ipAddress),
                    LowestLatency = CalculateLowestLatency(pingResults.Status, pingResults.Latency, ipAddress),
                    HighestLatency = CalculateHighestLatency(pingResults.Status, pingResults.Latency, ipAddress),
                    AverageLatency = CalculateAverageLatency(pingResults.Status, pingResults.Latency, ipAddress),
                    AllAveragesCombined = lastDataSet.AllAveragesCombined + pingResults.Latency,
                    PacketsLostTotal = CalculatePacketsLostTotal(pingResults.Status, ipAddress),
                    FailedPings = CalculateFailedPings(pingResults.Status, ipAddress),
                    ConnectionStatus = CalculateCurrentStatus(pingResults.Status, ipAddress),
                    TimeStampOfLastMajorChange = CalculateLastMajorChange(ipAddress, CalculateCurrentStatus(pingResults.Status, ipAddress))
                });
        }

        public static string CalculateCurrentStatus(IPStatus status, string ipAddress)
        {
            string responseCode;

            if (!LatencyMonitorDataStorage.LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    return "Down";
                }
                else
                {
                    return "Up";
                }
            }
            else
            {
                var dataSet = LatencyMonitorDataStorage.LiveData[ipAddress];

                if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) > 0.125
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) < .50)
                {
                    responseCode = "Unstable";
                    LatencyMonitorReport.ProcessLastMajorChange(ipAddress, responseCode);
                    return responseCode;
                }
                else if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) >= .50)
                {
                    responseCode = "Down";
                    LatencyMonitorReport.ProcessLastMajorChange(ipAddress, responseCode);
                    return responseCode;
                }
                else
                {
                    responseCode = "Up";
                    LatencyMonitorReport.ProcessLastMajorChange(ipAddress, responseCode);
                    return responseCode;
                }
            }
        }

        public static int CalculateLowestLatency(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            if (status == IPStatus.Success && latency <= lastDataSet.LowestLatency)
            {
                return latency;
            }
            else
            {
                return lastDataSet.LowestLatency;
            }
        }

        public static int CalculateHighestLatency(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            if (status == IPStatus.Success && latency >= lastDataSet.HighestLatency)
            {
                return latency;
            }
            else
            {
                return lastDataSet.HighestLatency;
            }
        }

        public static int CalculateAverageLatency(IPStatus status, int latency, string ipAddress)
        {
            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
            {
                return lastDataSet.AllAveragesCombined / lastDataSet.AverageLatencyDivider;
            }
            else if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency == 0)
            {
                return latency;
            }
            else
            {
                return lastDataSet.AverageLatency;
            }
        }

        public static int CalculatePacketsLostTotal(IPStatus status, string ipAddress)
        {
            if (!LatencyMonitorDataStorage.LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

                if (status != IPStatus.Success)
                {
                    return lastDataSet.PacketsLostTotal + 1;
                }
                else
                {
                    return lastDataSet.PacketsLostTotal;
                }
            }
        }

        public static int CalculateFailedPings(IPStatus status, string ipAddress)
        {
            if (!LatencyMonitorDataStorage.LiveData.ContainsKey(ipAddress))
            {
                if (status != IPStatus.Success)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var dataSet = LatencyMonitorDataStorage.LiveData[ipAddress];
                bool currentStatusCheck = status == IPStatus.Success;
                bool firstStatusCheck = dataSet.FirstOrDefault().Status == IPStatus.Success;
                bool maxedOutLiveData = dataSet.Count == 60;

                if ((!currentStatusCheck) && (!firstStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    return dataSet.LastOrDefault().FailedPings;
                }
                else if (currentStatusCheck && (!firstStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    return dataSet.LastOrDefault().FailedPings - 1;
                }
                else if ((!currentStatusCheck) && maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    return dataSet.LastOrDefault().FailedPings + 1;
                }
                else if (maxedOutLiveData)
                {
                    dataSet.RemoveAt(0);
                    return dataSet.LastOrDefault().FailedPings;
                }
                else if (!currentStatusCheck)
                {
                    return dataSet.LastOrDefault().FailedPings + 1;
                }
                else
                {
                    return dataSet.LastOrDefault().FailedPings;
                }
            }
        }

        public static int CalculateCounter(IPStatus status, long latency, string ipAddress)
        {
            if (!LatencyMonitorDataStorage.LiveData.ContainsKey(ipAddress))
            {
                if (status == IPStatus.Success && latency > 0)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

                if (status == IPStatus.Success && latency > 0 && lastDataSet.AverageLatency > 0)
                {
                    return lastDataSet.AverageLatencyDivider + 1;
                }
                else
                {
                    return lastDataSet.AverageLatencyDivider;
                }
            }
        }

        public static DateTime CalculateLastMajorChange(string ipAddress, string responseCode)
        {
            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Down"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently down, was previously down and its been half an hour
                // Updating the dictionary if the internet is still down and its been half an hour
                return DateTime.Now;
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Unstable"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently unstable, was previously unstable and its been half an hour
                // Updating the dictionary if the internet is still unstable and its been half an hour
                return DateTime.Now;
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the connection was down but is slowly becoming better
                return DateTime.Now;
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the connection was unstable and is now down completely
                return DateTime.Now;
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet just went down and has been good
                return DateTime.Now;
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet started being bad and has been good
                return DateTime.Now;
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the internet was down but is now good
                return DateTime.Now;
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the internet was unstable but is now good
                return DateTime.Now;
            }
            else
            {
                return lastDataSet.TimeStampOfLastMajorChange;
            }
        }

        public static void InitializeData(string ipAddress)
        {
            var pingResults = NetworkPing.PingTest(ipAddress);

            LatencyMonitorDataStorage
                .LiveData
                .Add(ipAddress, new List<LatencyMonitorData>
                { new()
                    {
                        IPAddress = ipAddress,
                        Status = pingResults.Status,
                        Latency = pingResults.Latency,
                        AverageLatencyDivider = 2,
                        LowestLatency = pingResults.Latency,
                        HighestLatency = pingResults.Latency,
                        AverageLatency = pingResults.Latency,
                        AllAveragesCombined = pingResults.Latency,
                        PacketsLostTotal = CalculatePacketsLostTotal(pingResults.Status, ipAddress),
                        FailedPings = CalculateFailedPings(pingResults.Status, ipAddress),
                        ConnectionStatus = CalculateCurrentStatus(pingResults.Status, ipAddress),
                        TimeStampOfLastMajorChange = DateTime.Now
                    }
                });

            var lastDataSet = LatencyMonitorDataStorage.LiveData[ipAddress].LastOrDefault();

            LatencyMonitorDataStorage
                .ReportData
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
