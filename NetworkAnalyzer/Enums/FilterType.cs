namespace NetworkAnalyzer.Enums;

internal enum FilterType
{
    TargetAddress = 1,
    TracerouteTarget = 2,
    CurrentLatency = 3,
    LowestLatency = 4,
    HighestLatency = 5,
    AverageLatency = 6,
    LostPacket = 7,
    TimeStamp = 8
}