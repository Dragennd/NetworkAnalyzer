using SQLite.Net2;

namespace NetworkAnalyzer.Apps.Models
{
    internal class IPScannerData
    {
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
        [PrimaryKey]
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

        [Column("ReportType")]
        public ReportType ReportType { get; set; }
    }
}
