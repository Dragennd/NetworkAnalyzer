namespace NetworkAnalyzer.Models;

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