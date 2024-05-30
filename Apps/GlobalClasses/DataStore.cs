using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using NetworkAnalyzer.Apps.Models;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public static class DataStore
    {
        // Store the data gathered by the IPScannerFunction class
        public static ConcurrentBag<IPScanData> ScanResults = new();

        // Lock the ScanResults ConcurrentBag to allow for thread-safe access to the bag
        public static readonly object ScanResultsLock = new object();
        
        // Store the IP Addresses/DNS Names used for scanning in the LatencyMonitorFunction class
        public static List<string> IPAddresses = new();

        // Store the host name resolved for IP Addresses entered in the LatencyMonitorFunction class
        public static List<IPAddress> ResolvedName = new();

        // Store the data gathered by the LatencyMonitorFunction class live as it is gathered
        public static Dictionary<string, List<LatencyMonitorData>> LiveData = new();

        // Store the major changes to the LiveData List which will ultimately be written to the report
        public static Dictionary<string, List<LatencyMonitorData>> ReportData = new();

        // Store the total number of packets sent for the duration of the Latency Monitor test
        public static int PacketsSent { get; set; }

        // Clear the Lists shown below prior to starting the next Latency Monitor test
        public static void ClearDataStorage()
        {
            IPAddresses.Clear();
            ResolvedName.Clear();
            LiveData.Clear();
            ReportData.Clear();
            PacketsSent = 0;
        }

        public class LatencyMonitorData
        {
            public string IPAddress { get; set; }
            public IPStatus Status { get; set; }
            public string ConnectionStatus { get; set; }
            public int Latency { get; set; }
            public int LowestLatency { get; set; }
            public int HighestLatency { get; set; }
            public int AverageLatency { get; set; }
            public int AllAveragesCombined { get; set; }
            public int PacketsLostTotal { get; set; }
            public int AverageLatencyDivider { get; set; }
            public int FailedPings { get; set; }
            public DateTime TimeStampOfLastMajorChange { get; set; }
        }

        // Response codes used for custom exception handling
        public enum ResponseCode
        {
            DataIsValid,
            EmptyDataCollectionException,
            InvalidIPAddressException,
            InputLessThanStartingPortException,
            EmptyInputException,
            InvalidInputException,
            BadRangeException
        }
    }
}