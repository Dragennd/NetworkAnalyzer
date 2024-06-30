using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal class LatencyMonitorManager
    {
        // Create an new entry in the ConcurrentDictionary containing the target and an empty ConcurrentQueue to initialize the session
        public async Task CreateSessionAsync(string targetName, LatencyMonitorData sessionData)
        {
            await Task.Run(() =>
            {
                int initialFailedPacketCount = 0;

                if (sessionData.Status == LatencyMonitorSessionStatus.Down)
                {
                    initialFailedPacketCount++;
                }

                var liveQueue = new ConcurrentQueue<LatencyMonitorData>();
                var reportQueue = new ConcurrentQueue<LatencyMonitorData>();

                liveQueue.Enqueue(sessionData);
                reportQueue.Enqueue(sessionData);

                LiveSessionData.TryAdd(targetName, liveQueue);
                ReportSessionData.TryAdd(targetName, reportQueue);
                FailedSessionPackets.TryAdd(targetName, initialFailedPacketCount);
            });
        }

        // Add a new entry to the ConcurrentBag for the specified target containing the latest session data
        public async Task<LatencyMonitorData> NewSessionDataAsync(string targetName, int latency, LatencyMonitorSessionStatus status, int lowestLatency, int highestLatency, int averageLatency, int totalLatency, int totalPacketsLost, bool failedPing, DateTime timeStamp, [Optional]int hop)
        {
            LatencyMonitorData sessionObject = new();

            // Set the session data into the sessionObject variable, then return it for processing
            sessionObject.TargetName = targetName;
            sessionObject.Status = status;
            sessionObject.Latency = latency;
            sessionObject.LowestLatency = lowestLatency;
            sessionObject.HighestLatency = highestLatency;
            sessionObject.AverageLatency = averageLatency;
            sessionObject.TotalLatency = totalLatency;
            sessionObject.TotalPacketsLost = totalPacketsLost;
            sessionObject.FailedPing = failedPing;
            sessionObject.TimeStamp = timeStamp;
            sessionObject.Hop = hop;

            return await Task.FromResult(sessionObject);
        }

        // Add a new entry to the ConcurrentBag for the specified target containing the latest session data
        public async Task<LatencyMonitorData> NewSessionDataAsync(string targetName, int hop)
        {
            LatencyMonitorData sessionObject = new();

            // Set the session data into the sessionObject variable, then return it for processing
            sessionObject.TargetName = targetName;
            sessionObject.Hop = hop;

            return await Task.FromResult(sessionObject);
        }

        // Remove the first entry in the LiveData ConcurrentQueue for the specified target once the amount of entries exceeds 60 entries
        public async Task RemoveSessionDataAsync(string targetName)
        {
            await Task.Run(() =>
            {
                var lastDataSet = LiveSessionData[targetName].LastOrDefault();
                //await CheckForDuplicates(targetName);

                if (LiveSessionData[targetName].Count() > 60)
                {
                    LiveSessionData[targetName].TryDequeue(out LatencyMonitorData entry);

                    lastDataSet.TotalLatency -= entry.Latency;

                    if (entry.FailedPing == true && FailedSessionPackets[targetName] > 0)
                    {
                        FailedSessionPackets[targetName]--;
                    }
                }
            });
        }

        // Add a LatencyMonitorData object to the ConcurrentDictionaries
        public async Task AddSessionDataAsync(string targetName, bool live, bool report, LatencyMonitorData sessionObject)
        {
            if (live)
            {
                await Task.Run(() =>
                {
                    LiveSessionData[targetName].Enqueue(sessionObject);
                });
            }

            if (report)
            {
                await Task.Run(() =>
                {
                    ReportSessionData[targetName].Enqueue(sessionObject);
                });
            }
        }

        //private async Task CheckForDuplicates(string targetName)
        //{
        //    await Task.Run(() =>
        //    {
        //        if (ReportSessionData[targetName].Count > 1)
        //        {
        //            var queue = ReportSessionData[targetName].GetEnumerator();
        //            var firstPosition = queue.Current.TimeStamp;
        //            queue.MoveNext();
        //            var secondPosition = queue.Current.TimeStamp;

        //            if (firstPosition == secondPosition)
        //            {
        //                ReportSessionData[targetName].TryDequeue(out _);
        //            }
        //        }
        //    });
        //}
    }
}
