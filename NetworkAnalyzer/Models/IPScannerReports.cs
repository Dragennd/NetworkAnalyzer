using NetworkAnalyzer.Enums;
using SQLite;

namespace NetworkAnalyzer.Models;

[Table("IPScannerReports")]
internal class IPScannerReports
{
    [PrimaryKey, AutoIncrement, Unique]
    [Column("ID")]
    public int ID { get; set; }

    [Column("ReportID")]
    public string ReportID { get; set; } = string.Empty;

    [Column("TotalScannableIPs")]
    public int TotalScannableIPs { get; set; }

    [Column("TotalActiveIPs")]
    public int TotalActiveIPs { get; set; }

    [Column("TotalInactiveIPs")]
    public int TotalInactiveIPs { get; set; }

    [Column("TotalDuration")]
    public string TotalDuration { get; set; }

    [Column("CreatedWhen")]
    public string CreatedWhen { get; set; } = string.Empty;

    [Column("ReportMode")]
    public ReportMode ReportMode { get; set; }
}