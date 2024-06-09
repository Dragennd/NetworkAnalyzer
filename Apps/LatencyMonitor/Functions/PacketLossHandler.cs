using System.Net.NetworkInformation;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class PacketLossHandler
    {
        // Determine the total number of packets lost by checking whether or not the ping requests were successful
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

        // Track the amount of failed pings in LiveData (max of 60 records kept at a time)
        // and use that data to determine the current status of the session (e.g. up, down, unstable)
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
    }
}
