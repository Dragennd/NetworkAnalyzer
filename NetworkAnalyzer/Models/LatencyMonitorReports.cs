using NetworkAnalyzer.Enums;
using SQLite;

namespace NetworkAnalyzer.Models;

[Table("LatencyMonitorReports")]
internal class LatencyMonitorReports
{
    [PrimaryKey, AutoIncrement, Unique]
    [Column("ID")]
    public int ID { get; set; }

    [Column("ReportID")]
    public string ReportID { get; set; }
    
    [Column("ReportFriendlyName")]
    public string? ReportFriendlyName { get; set; }

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

    [Column("ReportMode")]
    public ReportMode ReportMode { get; set; }
}