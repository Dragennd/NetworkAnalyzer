namespace NetworkAnalyzer.Apps.Models
{
    public class IPv4Info
    {
        public string? IPv4Address { get; set; }
        public string? SubnetMask { get; set; }
        public List<int>? IPBounds { get; set; } = new();
    }
}
