using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Enums;

internal enum FilterType
{
    [Display(Name = "User Defined Target")]
    TracerouteGUID = 1,
    
    [Display(Name = "Traceroute Target")]
    TargetGUID = 2,
    
    CurrentLatency = 3,
    LowestLatency = 4,
    HighestLatency = 5,
    AverageLatency = 6,
    FailedPing = 7,
    TimeStamp = 8
}