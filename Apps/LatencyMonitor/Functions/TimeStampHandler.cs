using NetworkAnalyzer.Apps.Models;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class TimeStampHandler
    {
        // Calculate the timestamp of the last major change to occur (changing to down or unstable generally)
        // and write that to the dictionaries
        public async Task<DateTime> CalculateTimeStampAsync(string targetName, LatencyMonitorSessionStatus status, LatencyMonitorSessionType type, bool initialization)
        {
            return await Task.FromResult(DateTime.Now);

            //if (initialization)
            //{
            //    return await Task.FromResult(DateTime.Now);
            //}
            //else
            //{
            //    var lastDataSet = LiveSessionData[targetName].LastOrDefault();

            //    if ((status == LatencyMonitorSessionStatus.Down && lastDataSet.Status == LatencyMonitorSessionStatus.Down && DateTime.Now >= lastDataSet.TimeStamp.AddMinutes(30)) ||
            //        (status == LatencyMonitorSessionStatus.Down && lastDataSet.Status == LatencyMonitorSessionStatus.Down && type == LatencyMonitorSessionType.Report))
            //    {
            //        // If its currently down, was previously down and its been half an hour
            //        // Updating the dictionary if the internet is still down and its been half an hour
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if ((status == LatencyMonitorSessionStatus.Unstable && lastDataSet.Status == LatencyMonitorSessionStatus.Unstable && DateTime.Now >= lastDataSet.TimeStamp.AddMinutes(30)) ||
            //             (status == LatencyMonitorSessionStatus.Unstable && lastDataSet.Status == LatencyMonitorSessionStatus.Unstable && type == LatencyMonitorSessionType.Report))
            //    {
            //        // If its currently unstable, was previously unstable and its been half an hour
            //        // Updating the dictionary if the internet is still unstable and its been half an hour
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Unstable
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Down)
            //    {
            //        // If the connection was down but is slowly becoming better
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Down
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Unstable)
            //    {
            //        // If the connection was unstable and is now down completely
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Down
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Up)
            //    {
            //        // If the internet just went down and has been good
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Unstable
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Up)
            //    {
            //        // If the internet started being bad and has been good
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Up
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Down)
            //    {
            //        // If the internet was down but is now good
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Up
            //        && lastDataSet.Status == LatencyMonitorSessionStatus.Unstable)
            //    {
            //        // If the internet was unstable but is now good
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else if (status == LatencyMonitorSessionStatus.Up && lastDataSet.Status == LatencyMonitorSessionStatus.Up && type == LatencyMonitorSessionType.Report)
            //    {
            //        return await Task.FromResult(DateTime.Now);
            //    }
            //    else
            //    {
            //        return await Task.FromResult(lastDataSet.TimeStamp);
            //    }
            //}
        }
    }
}
