namespace NetworkAnalyzer.Apps.Models
{
    internal enum TracerouteStatus
    {
        Completed = 1,
        Failed = 2,
        Unresolved = 3 // Only use this if the traceroute fails completely
    }
}
