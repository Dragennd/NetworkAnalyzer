using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    internal static class LatencyMonitorManager
    {
        // Create an new entry in the ConcurrentDictionary containing the target and an empty ConcurrentQueue to initialize the session
        public static async Task CreateSessionAsync(string targetName, LatencyMonitorData sessionData)
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

        // Add a new entry to the ConcurrentQueue for the specified target containing the latest session data
        // Used specifically for when a ping hop fails to respond so as to add a placeholder
        public static async Task<LatencyMonitorData> NewSessionDataAsync(string targetName, int hop)
        {
            LatencyMonitorData sessionObject = new();

            // Set the session data into the sessionObject variable, then return it for processing
            sessionObject.TargetName = targetName;
            sessionObject.Hop = hop;

            return await Task.FromResult(sessionObject);
        }

        // Remove the first entry in the LiveData ConcurrentQueue for the specified target once the amount of entries exceeds 60 entries
        public static async Task RemoveSessionDataAsync(string targetName)
        {
            await Task.Run(() =>
            {
                var lastDataSet = LiveSessionData[targetName].Last();

                if (LiveSessionData[targetName].Count > 60)
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
        public static async Task AddSessionDataAsync(string targetName, bool live, bool report, LatencyMonitorData sessionObject)
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
    }
}
