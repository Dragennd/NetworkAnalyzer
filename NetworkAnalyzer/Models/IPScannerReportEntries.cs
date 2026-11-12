using SQLite;

namespace NetworkAnalyzer.Models;

[Table("IPScannerReportEntries")]
internal class IPScannerReportEntries
{
    [PrimaryKey, AutoIncrement, Unique]
    [Column("ID")]
    public int ID { get; set; }

    [Column("ReportID")]
    public string ReportID { get; set; }

    [Column("DeviceName")]
    public string? Name { get; set; }

    [Column("IPAddress")]
    public string? IPAddress { get; set; }

    [Column("MACAddress")]
    public string? MACAddress { get; set; }

    [Column("Manufacturer")]
    public string? Manufacturer { get; set; }

    [Column("RDPEnabled")]
    public bool RDPEnabled { get; set; } = false;

    [Column("SMBEnabled")]
    public bool SMBEnabled { get; set; } = false;

    [Column("SSHEnabled")]
    public bool SSHEnabled { get; set; } = false;
}