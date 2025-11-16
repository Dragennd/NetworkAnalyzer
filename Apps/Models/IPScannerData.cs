using SQLite;

namespace NetworkAnalyzer.Apps.Models
{
    internal class IPScannerData
    {
        public int ID { get; set; }
        public string ReportID { get; set; }
        public string? Name { get; set; }
        public string? IPAddress { get; set; }
        public string? MACAddress { get; set; }
        public string? Manufacturer { get; set; }
        public bool RDPEnabled { get; set; } = false;
        public bool SMBEnabled { get; set; } = false;
        public bool SSHEnabled { get; set; } = false;
    }

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
}
