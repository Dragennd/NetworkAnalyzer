namespace NetworkAnalyzer.Apps.Models
{
    internal class IPv4Info
    {
        public string? IPv4Address { get; set; }
        public string? SubnetMask { get; set; }
        public string? NetworkAddressWithCIDR { get; set; }
        public List<int>? IPBounds { get; set; } = new();
    }
}
