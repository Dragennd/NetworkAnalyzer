using System.ComponentModel;
using SQLite;

namespace NetworkAnalyzer.Models;

[Table("LatencyMonitorReportEntries")]
public class LatencyMonitorReportEntries
{
    [PrimaryKey, AutoIncrement, Unique]
    [Column("ID")]
    public int ID { get; set; }

    [Column("ReportID")]
    public string ReportID { get; set; }

    [Column("DisplayName")]
    public string DisplayName { get; set; }

    [Column("TargetName")]
    public string TargetName { get; set; }

    [Column("TargetAddress")]
    public string TargetAddress { get; set; }

    [Column("TargetStatus")]
    public LatencyMonitorTargetStatus TargetStatus { get; set; }

    [Column("TargetGUID")]
    public string TargetGUID { get; set; }

    [Column("TracerouteGUID")]
    public string TracerouteGUID { get; set; }

    [Column("Hop")]
    public int Hop { get; set; }

    [Column("AverageLatencyCounter")]
    public int AverageLatencyCounter { get; set; }

    [Column("CurrentLatency")]
    public string CurrentLatency { get; set; }

    public string DisplayCurrentLatency
    {
        get
        {
            if (CurrentLatency != "-" && int.Parse(CurrentLatency) >= 1)
            {
                return $"{CurrentLatency} ms";
            }
            
            if (CurrentLatency != "-" && int.Parse(CurrentLatency) < 1)
            {
                return "<1 ms";
            }

            return CurrentLatency;
        }
    }

    [Column("LowestLatency")]
    public string LowestLatency { get; set; }

    public string DisplayLowestLatency
    {
        get
        {
            if (LowestLatency != "-" && int.Parse(LowestLatency) >= 1)
            {
                return $"{LowestLatency} ms";
            }
            
            if (LowestLatency != "-" && int.Parse(LowestLatency) < 1)
            {
                return "<1 ms";
            }

            return LowestLatency;
        }
    }

    [Column("HighestLatency")]
    public string HighestLatency { get; set; }

    public string DisplayHighestLatency
    {
        get
        {
            if (HighestLatency != "-" && int.Parse(HighestLatency) >= 1)
            {
                return $"{HighestLatency} ms";
            }
            
            if (HighestLatency != "-" && int.Parse(HighestLatency) < 1)
            {
                return "<1 ms";
            }

            return HighestLatency;
        }
    }

    [Column("AverageLatency")]
    public string AverageLatency { get; set; }

    public string DisplayAverageLatency
    {
        get
        {
            if (AverageLatency != "-" && int.Parse(AverageLatency) >= 1)
            {
                return $"{AverageLatency} ms";
            }
            
            if (AverageLatency != "-" && int.Parse(AverageLatency) < 1)
            {
                return "<1 ms";
            }

            return AverageLatency;
        }
    }

    [Column("TotalPacketsLost")]
    public string TotalPacketsLost { get; set; }

    [Column("TotalLatency")]
    public int TotalLatency { get; set; }
    
    [Column("Jitter")]
    public int Jitter { get; set; }
    
    public string DisplayJitter => $"{Jitter} ms";
    
    [Column("StandardDeviation")]
    public string StandardDeviation { get; set; }

    public string DisplayStandardDeviation
    {
        get
        {
            if (StandardDeviation != "-")
            {
                return $"{StandardDeviation} ms";
            }

            return StandardDeviation;
        }
    }

    [Column("FailedPing")]
    public bool FailedPing { get; set; }

    [Column("IsUserDefinedTarget")]
    public bool IsUserDefinedTarget { get; set; }

    [Column("TimeStamp")]
    public string TimeStamp { get; set; }
}