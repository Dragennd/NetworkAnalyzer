using NetworkAnalyzer.Apps.Models;
using System.Net.NetworkInformation;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    internal class StatusHandler
    {
        // Process the current status into LiveData
        public async Task<LatencyMonitorSessionStatus> CalculateCurrentStatusAsync(IPStatus status, string targetName, bool initialization)
        {
            LatencyMonitorSessionStatus sessionStatus = LatencyMonitorSessionStatus.None;

            if (initialization)
            {
                if (status == IPStatus.Success)
                {
                    sessionStatus = LatencyMonitorSessionStatus.Up;
                }
                else
                {
                    sessionStatus = LatencyMonitorSessionStatus.Down;
                }
            }
            else
            {
                var lastDataSet = LiveSessionData[targetName];
                var failedPingsCount = FailedSessionPackets[targetName];

                if ((double)failedPingsCount > 0 && (double)failedPingsCount / lastDataSet.Count > 0.125 && (double)failedPingsCount / lastDataSet.Count < 0.50)
                {
                    sessionStatus = LatencyMonitorSessionStatus.Unstable;
                }
                else if ((double)failedPingsCount > 0 && (double)failedPingsCount / lastDataSet.Count > 0.50)
                {
                    sessionStatus = LatencyMonitorSessionStatus.Down;
                }
                else
                {
                    sessionStatus = LatencyMonitorSessionStatus.Up;
                }
            }

            return await Task.FromResult(sessionStatus);
        }

        // Used to process major changes into the ReportData dictionary
        public async Task ProcessLastMajorChange(string targetName)
        {
            LatencyMonitorManager manager = new();

            var lastReportDataSet = ReportSessionData[targetName].LastOrDefault();
            var lastLiveDataSet = LiveSessionData[targetName].LastOrDefault();

            if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Down
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Down
                && lastLiveDataSet.TimeStamp >= lastReportDataSet.TimeStamp.AddMinutes(30))
            {
                // If its currently down, was previously down and its been half an hour
                // Updating the dictionary if the internet is still down and its been half an hour
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Unstable
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Unstable
                && lastLiveDataSet.TimeStamp >= lastReportDataSet.TimeStamp.AddMinutes(30))
            {
                // If its currently unstable, was previously unstable and its been half an hour
                // Updating the dictionary if the internet is still unstable and its been half an hour
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Unstable
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Down)
            {
                // If the connection was down but is slowly becoming better
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Unstable
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Up)
            {
                // If the internet started being bad and has been good
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Down
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Unstable)
            {
                // If the connection was unstable and is now down completely
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Down
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Up)
            {
                // If the internet just went down and has been good
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Up
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Down)
            {
                // If the internet was down but is now good
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
            else if (lastLiveDataSet.Status == LatencyMonitorSessionStatus.Up
                && lastReportDataSet.Status == LatencyMonitorSessionStatus.Unstable)
            {
                // If the internet was unstable but is now good
                await manager.AddSessionDataAsync(targetName, false, true, lastLiveDataSet);
            }
        }
    }
}
