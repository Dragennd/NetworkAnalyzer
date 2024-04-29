using System.Net;
using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public static class LatencyMonitorDataStorage
    {
        public static Dictionary<string, List<LatencyMonitorData>> LiveData = new();

        public static Dictionary<string, List<LatencyMonitorData>> ReportData = new();

        public static List<string> IPAddresses = new();

        public static List<IPAddress> ResolvedName = new();

        public static int PacketsSent { get; set; }

        public static void ClearDataStorage()
        {
            IPAddresses.Clear();
            ResolvedName.Clear();
            LiveData.Clear();
            ReportData.Clear();
            PacketsSent = 0;
        }
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
