using System.Net.NetworkInformation;

namespace NetworkAnalyzer.Apps.Models
{
    public class LatencyMonitorData
    {
        public string IPAddress { get; set; }
        public IPStatus Status { get; set; } // Not displayed on the UI
        public string ConnectionStatus { get; set; }
        public int Latency { get; set; }
        public int LowestLatency { get; set; }
        public int HighestLatency { get; set; }
        public int AverageLatency { get; set; }
        public int AllAveragesCombined { get; set; } // Not displayed on the UI
        public int PacketsLostTotal { get; set; }
        public int AverageLatencyDivider { get; set; } // Not displayed on the UI
        public int FailedPings { get; set; } // Not displayed on the UI
        public DateTime TimeStampOfLastMajorChange { get; set; } // Not displayed on the UI
    }
}
