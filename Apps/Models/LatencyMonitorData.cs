using SQLite.Net2;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace NetworkAnalyzer.Apps.Models
{
    internal class LatencyMonitorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LatencyMonitorData()
        {

        }

        [Required]
        public string TargetName { get; set; } = string.Empty;
        public string UserDefinedTarget { get; set; } = string.Empty;
        public string DNSHostName { get; set; } = string.Empty;
        public string SpecifiedTargetName { get; set; } = string.Empty;
        public LatencyMonitorTargetStatus TargetStatus { get; set; }

        private string latency = "-";
        public string Latency
        {
            get
            {
                return latency;
            }
            set
            {
                latency = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(Latency)));
            }
        }

        private string lowestLatency = "-";
        public string LowestLatency
        {
            get
            {
                return lowestLatency;
            }
            set
            {
                lowestLatency = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(LowestLatency)));
            }
        }

        private string highestLatency = "-";
        public string HighestLatency
        {
            get
            {
                return highestLatency;
            }
            set
            {
                highestLatency = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(HighestLatency)));
            }
        }

        private string averageLatency = "-";
        public string AverageLatency
        {
            get
            {
                return averageLatency;
            }
            set
            {
                averageLatency = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(AverageLatency)));
            }
        }

        private string totalPacketsLost = "-";
        public string TotalPacketsLost
        {
            get
            {
                return totalPacketsLost;
            }
            set
            {
                totalPacketsLost = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(TotalPacketsLost)));
            }
        }

        public int Hop { get; set; } = 0;
        public int FailedHopCounter { get; set; } = 0;
        public int AverageLatencyCounter { get; set; } = 0;
        public int TotalLatency { get; set; } = 0;

        private bool failedPing = false;
        public bool FailedPing
        {
            get
            {
                return failedPing;
            }
            set
            {
                failedPing = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(FailedPing)));
            }
        }

        public DateTime TimeStamp { get; set; }

        private void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
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
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ReportID")]
        public string ReportID { get; set; }

        [Column("TargetName")]
        public string TargetName { get; set; }

        [Column("DNSName")]
        public string DNSHostName { get; set; }

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
