using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Functions
{
    public class TimeStampHandler
    {
        // Calculate the timestamp of the last major change to occur (changing to down or unstable generally)
        // and write that to the dictionaries
        public static async Task<DateTime> CalculateMajorChangeTimeStampAsync(string ipAddress, string responseCode)
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
    }
}
