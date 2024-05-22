using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkAnalyzer.Apps.GlobalClasses
{
    public static class DataStore
    {
        public static ConcurrentBag<IPScanData> ScanResults = new();

        public static readonly object ScanResultsLock = new object();
        
        public static List<string> IPAddresses = new();

        public static List<IPAddress> ResolvedName = new();

        public static Dictionary<string, List<LatencyMonitorData>> LiveData = new();

        public static Dictionary<string, List<LatencyMonitorData>> ReportData = new();

        public static int PacketsSent { get; set; }

        public static void ClearDataStorage()
        {
            IPAddresses.Clear();
            ResolvedName.Clear();
            LiveData.Clear();
            ReportData.Clear();
            PacketsSent = 0;
        }

        public class IPScanData
        {
            public string? Name {  get; set; }
            public string? IPAddress { get; set; }
            public string? MACAddress { get; set; }
            public string? Manufacturer { get; set; }
            public bool RDPEnabled { get; set; } = false;
            public bool SMBEnabled { get; set; } = false;
            public bool SSHEnabled { get; set; } = false;
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

        public enum ResponseCode
        {
            Data_Is_Valid,
            Empty_Data_Collection_Exception,
            Invalid_IP_Address_Exception,
            Input_Less_Than_Starting_Port_Exception,
            Empty_Input_Exception
        }
    }
}