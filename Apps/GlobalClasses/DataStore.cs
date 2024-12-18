using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    internal static class DataStore
    {
        #region Global
        // Event used to handle property updates for the datastore class
        public static event PropertyChangedEventHandler PropertyChanged;

        // Specifies the root directory used by the Network Analyzer
        public static string DataDirectory { get; } = @"C:\Network Analyzer\";

        // Specifies the directory used to store reports generated by the Network Analyzer
        public static string ReportDirectory { get; } = $@"{DataDirectory}Reports\";

        // Specifies the directory used to store config and ini files
        public static string ConfigDirectory { get; } = $@"{DataDirectory}Config\";

        // Specifies the directory used to store log files
        public static string LogDirectory { get; } = $@"{DataDirectory}Logs\";

        // Path to the database file in the Network Analyzer directory
        public static string DatabasePath { get; } = $@"{ConfigDirectory}NetworkAnalyzerDB.db";

        // Path to the database file within the app build path
        public static string LocalDatabasePath { get; } = "NetworkAnalyzer.Data.NetworkAnalyzerDB.db";

        // Current build of the application - used to compare to GitHub for notifying the user if a newer version is available
        public static string CurrentBuild { get; } = "1.6.1";

        // Release date for the current build of the application - checked against the manifest in GitHub
        public static string ReleaseDate { get; } = "11/27/2024";
        #endregion

        #region IP Scanner Data
        // Store the IP Bounds for the current session
        public static ConcurrentBag<IPv4Info> ActiveSubnets = new();

        // Represents the total amount of IP Addresses available to scan in the provided range
        private static int _totalSizeOfSubnetToScan;
        public static int TotalSizeOfSubnetToScan
        {
            get => _totalSizeOfSubnetToScan;
            set
            {
                if (_totalSizeOfSubnetToScan != value)
                {
                    _totalSizeOfSubnetToScan = value;

                    OnStaticPropertyChanged(nameof(TotalSizeOfSubnetToScan));
                }
            }
        }

        // Represents the total amount of IP Addresses which have been successfully pinged
        private static int _totalActiveIPAddresses;
        public static int TotalActiveIPAddresses
        {
            get => _totalActiveIPAddresses;
            set
            {
                if (_totalActiveIPAddresses != value)
                {
                    _totalActiveIPAddresses = value;

                    OnStaticPropertyChanged(nameof(TotalActiveIPAddresses));
                }
            }
        }

        // Represents the total amount of IP Addresses which have failed to respond to pings
        private static int _totalInactiveIPAddresses;
        public static int TotalInactiveIPAddresses
        {
            get => _totalInactiveIPAddresses;
            set
            {
                if (_totalInactiveIPAddresses != value)
                {
                    _totalInactiveIPAddresses = value;

                    OnStaticPropertyChanged(nameof(TotalInactiveIPAddresses));
                }
            }
        }

        // Stores the duration of the scan
        public static string TotalScanDuration { get; set; } = string.Empty;

        // Datetime the scan was performed
        public static string DateScanWasPerformed { get; set; } = string.Empty;

        // Report number for the currently running IP Scanner session
        public static string IPScannerReportID { get; set; } = string.Empty;

        // Report type for the currently running IP Scanner session
        public static ReportType IPScannerReportType { get; set; } = ReportType.ICMP;
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

        // Report number for the currently running Latency Monitor session
        public static string LatencyMonitorReportID { get; set; } = string.Empty;

        // Store the total number of packets sent for the duration of the Latency Monitor test
        public static int PacketsSent { get; set; }

        // Store the final duration in  of the monitoring session 
        public static string TotalDuration { get; set; } = "00.00:00:00";

        // Store the start time for the current Latency Monitor session
        public static string? StartTime { get; set; }

        // Store the end time for the current Latency Monitor session
        public static string? EndTime { get; set; }

        // Store the last mode with which data was stored from a Latency Monitor session
        public static ReportType LatencyMonitorReportType { get; set; } = ReportType.UserTargets;

        // Clear the Lists shown below prior to starting the next Latency Monitor test
        public static void ClearDataStorage()
        {
            IPAddresses.Clear();
            ResolvedName.Clear();
            LiveSessionData.Clear();
            ReportSessionData.Clear();
            FailedSessionPackets.Clear();
            PacketsSent = 0;
            TotalDuration = "00.00:00:00";
            StartTime = string.Empty;
            EndTime = string.Empty;
        }
        #endregion

        #region Reports Data
        // Store list of all reports in the ReportDirectory
        public static ConcurrentBag<ReportExplorerData> ReportsData = new();
        #endregion Reports Data

        #region Private Methods
        public static void OnStaticPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}