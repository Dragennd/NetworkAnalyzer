using SQLite.Net2;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Apps.Models
{
    internal class LatencyMonitorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LatencyMonitorData()
        {

        }

        [Required]
        public string DisplayName { get; set; } = "N/A"; // Friendly name for the target
        public string TargetName { get; set; } = "N/A"; // DNS name for the target
        public string TargetAddress { get; set; } = "N/A"; // IP Address for the target
        public string ReportID { get; set; } = string.Empty;
        public string TracerouteGUID { get; set; } = string.Empty;
        public string TargetGUID { get; set; } = string.Empty;
        public LatencyMonitorTargetStatus TargetStatus { get; set; } = LatencyMonitorTargetStatus.None;

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
        public bool IsUserDefinedTarget { get; set; } = false;

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

        [Column("SuccessfullyCompleted")]
        public string SuccessfullyCompleted { get; set; } = "false";
    }

    [Table("LatencyMonitorReportEntries")]
    internal class LatencyMonitorReportEntries
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ReportID")]
        public string ReportID { get; set; }

        [Column("DisplayName")]
        public string DisplayName { get; set; }

        [Column("TargetName")]
        public string TargetName { get; set; }

        [Column("TargetAddress")]
        public string TargetAddress { get; set; }

        [Column("TargetStatus")]
        public LatencyMonitorTargetStatus TargetStatus { get; set; }

        [Column("TargetGUID")]
        public string TargetGUID { get; set; }

        [Column("TracerouteGUID")]
        public string TracerouteGUID { get; set; }

        [Column("Hop")]
        public int Hop { get; set; }

        [Column("FailedHopCounter")]
        public int FailedHopCounter { get; set; }

        [Column("AverageLatencyCounter")]
        public int AverageLatencyCounter { get; set; }

        [Column("LowestLatency")]
        public string LowestLatency { get; set; }

        [Column("HighestLatency")]
        public string HighestLatency { get; set; }

        [Column("AverageLatency")]
        public string AverageLatency { get; set; }

        [Column("TotalPacketsLost")]
        public string TotalPacketsLost { get; set; }

        [Column("TotalLatency")]
        public int TotalLatency { get; set; }

        [Column("FailedPing")]
        public bool FailedPing { get; set; }

        [Column("IsUserDefinedTarget")]
        public bool IsUserDefinedTarget { get; set; }

        [Column("TimeStamp")]
        public string TimeStamp { get; set; }
    }
}
