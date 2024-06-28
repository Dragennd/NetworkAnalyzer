using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Apps.Models
{
    internal class LatencyMonitorData
    {
        [Required]
        public string TargetName { get; set; }
        public LatencyMonitorSessionStatus Status { get; set; }
        public int Hop { get; set; }
        public int Latency { get; set; }
        public int LowestLatency { get; set; }
        public int HighestLatency { get; set; }
        public int AverageLatency { get; set; }
        public int TotalLatency { get; set; }
        public int TotalPacketsLost { get; set; }
        public bool FailedPing { get; set; }
        public DateTime TimeStamp { get; set; }

        public LatencyMonitorData()
        {
            TargetName = string.Empty;
        }
    }
}
