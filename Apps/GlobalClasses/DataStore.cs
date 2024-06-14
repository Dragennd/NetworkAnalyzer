using System.Collections.Concurrent;
using System.Net;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public static class DataStore
    {
        #region Global
        // Specifies the root directory used by the Network Analyzer
        public static readonly string DataDirectory = @"C:\Network Analyzer\";

        // Specifies the directory used to store reports generated by the Network Analyzer
        public static readonly string ReportDirectory = $@"{DataDirectory}\Reports";

        // Specifies the directory used to store config and ini files - NOT CURRENTLY IN USE
        public static readonly string ConfigDirectory = $@"{DataDirectory}\Config";

        // Current build of the application - used to compare to GitHub for notifying the user if a newer version is available
        public static readonly string CurrentBuild = "1.4.0";
        #endregion

        #region IP Scanner Data
        // Store the data gathered by the IPScannerFunction class
        public static ConcurrentBag<IPScannerData> ScanResults = new();

        // Lock the ScanResults ConcurrentBag to allow for thread-safe access to the bag
        public static readonly object ScanResultsLock = new object();
        #endregion

        #region Latency Monitor Data
        // Store the data gathered by the LatencyMonitorFunction class live as it is gathered
        public static ConcurrentDictionary<string, ConcurrentQueue<LatencyMonitorData>> LiveSessionData = new();

        // Store the major changes to the LiveData List which will ultimately be written to the report
        public static ConcurrentDictionary<string, ConcurrentQueue<LatencyMonitorData>> ReportSessionData = new();

        // Store the current amount of failed pings per target for use with calculating the status
        public static ConcurrentDictionary<string, int> FailedSessionPackets = new();

        // Store the IP Addresses/DNS Names used for scanning in the LatencyMonitorFunction class
        public static List<string> IPAddresses = new();

        // Store the host name resolved for IP Addresses entered in the LatencyMonitorFunction class
        public static List<IPAddress> ResolvedName = new();

        // Store the total number of packets sent for the duration of the Latency Monitor test
        public static int PacketsSent { get; set; }

        // Store the final duration in  of the monitoring session 
        public static string TotalDuration { get; set; } = "00.00:00:00";

        // Clear the Lists shown below prior to starting the next Latency Monitor test
        public static void ClearDataStorage()
        {
            IPAddresses.Clear();
            ResolvedName.Clear();
            LiveSessionData.Clear();
            ReportSessionData.Clear();
            PacketsSent = 0;
        }
        #endregion
    }
}