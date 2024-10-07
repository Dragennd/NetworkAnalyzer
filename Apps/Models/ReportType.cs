namespace NetworkAnalyzer.Apps.Models
{
    internal enum ReportType
    {
        None = 0,
        UserTargets = 1,
        Traceroute = 2,
        ICMP = 3
    }

    internal enum ReportMode
    {
        None = 0,
        LatencyMonitor = 1,
        IPScanner = 2,
    }
}
