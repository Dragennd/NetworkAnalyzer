using System.Net.NetworkInformation;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class StatusHandler
    {
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

                // If the session is unstable, write the relevant data to both LiveData and ReportData
                if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) > 0.125
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) < .50)
                {
                    responseCode = "Unstable";
                    ProcessLastMajorChange(ipAddress, responseCode);
                }
                // If the session is down, write the relevant data to both LiveData and ReportData
                else if (dataSet.LastOrDefault().FailedPings > 0
                    && (dataSet.LastOrDefault().FailedPings / dataSet.Count) >= .50)
                {
                    responseCode = "Down";
                    ProcessLastMajorChange(ipAddress, responseCode);
                }
                // If the session is online, write the relevant data to both LiveData and ReportData
                else
                {
                    responseCode = "Up";
                    ProcessLastMajorChange(ipAddress, responseCode);
                }
            }

            return await Task.FromResult(responseCode);
        }

        // Used to process major changes into the ReportData dictionary
        public static void ProcessLastMajorChange(string ipAddress, string responseCode)
        {
            var lastDataSet = ReportData[ipAddress].LastOrDefault();

            if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Down"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently down, was previously down and its been half an hour
                // Updating the dictionary if the internet is still down and its been half an hour
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Unstable"
                && DateTime.Now >= lastDataSet.TimeStampOfLastMajorChange.AddMinutes(30))
            {
                // If its currently unstable, was previously unstable and its been half an hour
                // Updating the dictionary if the internet is still unstable and its been half an hour
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the connection was down but is slowly becoming better
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the connection was unstable and is now down completely
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Down"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet just went down and has been good
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Unstable"
                && lastDataSet.ConnectionStatus == "Up")
            {
                // If the internet started being bad and has been good
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Down")
            {
                // If the internet was down but is now good
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
            else if (responseCode == "Up"
                && lastDataSet.ConnectionStatus == "Unstable")
            {
                // If the internet was unstable but is now good
                LatencyMonitorFunction.WriteToReportData(ipAddress);
            }
        }
    }
}
