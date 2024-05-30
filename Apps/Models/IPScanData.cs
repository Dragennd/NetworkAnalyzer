namespace NetworkAnalyzer.Apps.Models
{
    public class IPScanData
    {
        public string? Name { get; set; }
        public string? IPAddress { get; set; }
        public string? MACAddress { get; set; }
        public string? Manufacturer { get; set; }
        public bool RDPEnabled { get; set; } = false;
        public bool SMBEnabled { get; set; } = false;
        public bool SSHEnabled { get; set; } = false;
    }
}
