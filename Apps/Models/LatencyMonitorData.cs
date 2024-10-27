using SQLite.Net2;
using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Apps.Models
{
    internal class LatencyMonitorData
    {
        [Required]
        public string TargetName { get; set; }
        public string DNSHostName { get; set; }
        public LatencyMonitorSessionStatus Status { get; set; }
        public int Hop { get; set; }
        public int FailedHopCounter { get; set; }
        public int Latency { get; set; }
        public int LowestLatency { get; set; }
        public int HighestLatency { get; set; }
        public int AverageLatency { get; set; }
        public int AverageLatencyCounter { get; set; }
        public int TotalLatency { get; set; }
        public int TotalPacketsLost { get; set; }
        public bool FailedPing { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    [Table("LatencyMonitorReports")]
    internal class LatencyMonitorReports
    {
        [PrimaryKey]
        [Column("ReportID")]
        public string ReportID { get; set; }

        [Column("StartedWhen")]
        public string? StartedWhen { get; set; }

        [Column("CompletedWhen")]
        public string? CompletedWhen { get; set; }

        [Column("TotalDuration")]
        public string? TotalDuration { get; set; }

        [Column("TotalPacketsSent")]
        public int TotalPacketsSent { get; set; }

        [Column("ReportType")]
        public ReportType ReportType { get; set; }

        [Column("SuccessfullyCompleted")]
        public string SuccessfullyCompleted { get; set; } = "false";
    }

    [Table("LatencyMonitorReportSnapshots")]
    internal class LatencyMonitorReportSnapshots
    {
        [PrimaryKey]
        [Column("ReportID")]
        public string ReportID { get; set; }

        [PrimaryKey]
        [Column("TargetName")]
        public string TargetName { get; set; }

        [Column("DNSName")]
        public string DNSHostName { get; set; }

        [Column("Status")]
        public LatencyMonitorSessionStatus Status { get; set; }

        [Column("Hop")]
        public int Hop { get; set; }

        [PrimaryKey]
        [Column("FailedHopCounter")]
        public int FailedHopCounter { get; set; }

        [Column("LowestLatency")]
        public int LowestLatency { get; set; }

        [Column("HighestLatency")]
        public int HighestLatency { get; set; }

        [Column("AverageLatency")]
        public int AverageLatency { get; set; }

        [Column("TotalPacketsLost")]
        public int TotalPacketsLost { get; set; }

        [Column("TimeStamp")]
        public string TimeStamp { get; set; }

        [Column("PacketsSent")]
        public int PacketsSent { get; set; }

        [Column("Duration")]
        public string Duration { get; set; }
    }

    [Table("LatencyMonitorReportEntries")]
    internal class LatencyMonitorReportEntries
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ReportID")]
        public string ReportID { get; set; }

        [Column("TargetName")]
        public string TargetName { get; set; }

        [Column("DNSName")]
        public string DNSHostName { get; set; }

        [Column("Status")]
        public LatencyMonitorSessionStatus Status { get; set; }

        [Column("Hop")]
        public int Hop { get; set; }

        [Column("FailedHopCounter")]
        public int FailedHopCounter { get; set; }

        [Column("LowestLatency")]
        public int LowestLatency { get; set; }

        [Column("HighestLatency")]
        public int HighestLatency { get; set; }

        [Column("AverageLatency")]
        public int AverageLatency { get; set; }

        [Column("TotalPacketsLost")]
        public int TotalPacketsLost { get; set; }

        [Column("TimeStamp")]
        public string TimeStamp { get; set; }
    }

    [Table("LatencyMonitorTargetProfiles")]
    internal class LatencyMonitorTargetProfiles
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ProfileName")]
        public string ProfileName { get; set; }

        [Column("ReportType")]
        public ReportType ReportType { get; set; }

        [Column("Hops")]
        public int Hops { get; set; }

        [Column("Target1")]
        public string? Target1 { get; set; }

        [Column("Target2")]
        public string? Target2 { get; set; }

        [Column("Target3")]
        public string? Target3 { get; set; }

        [Column("Target4")]
        public string? Target4 { get; set; }

        [Column("Target5")]
        public string? Target5 { get; set; }
    }
}
