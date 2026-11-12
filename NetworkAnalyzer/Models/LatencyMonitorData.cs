using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NetworkAnalyzer.Models;

internal class LatencyMonitorData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int ID { get; set; }
    [Required]
    public string DisplayName { get; set; } = "N/A"; // Friendly name for the target
    public string TargetName { get; set; } = "N/A"; // DNS name for the target
    public string TargetAddress { get; set; } = "N/A"; // IP Address for the target
    public string ReportID { get; set; } = string.Empty;
    public string TracerouteGUID { get; set; } = string.Empty;
    public string TargetGUID { get; set; } = string.Empty;
    public LatencyMonitorTargetStatus TargetStatus { get; set; } = LatencyMonitorTargetStatus.None;
    public string Latency
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(Latency)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayLatency)));
        }
    } = "-";
    public string DisplayLatency
    {
        get
        {
            if (Latency != "-" && int.Parse(Latency) >= 1)
            {
                return $"{Latency} ms";
            }
            
            if (Latency != "-" && int.Parse(Latency) < 1)
            {
                return "<1 ms";
            }

            return Latency;
        }
    }
    public string LowestLatency
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(LowestLatency)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayLowestLatency)));
        }
    } = "-";

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
    public string HighestLatency
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(HighestLatency)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayHighestLatency)));
        }
    } = "-";

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
    public string AverageLatency
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(AverageLatency)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayAverageLatency)));
        }
    } = "-";

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
    public string TotalPacketsLost
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(TotalPacketsLost)));
        }
    } = "-";
    public int Hop
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(Hop)));
        }
    } = 0;
    public int AverageLatencyCounter { get; set; } = 0;
    public int TotalLatency { get; set; } = 0;
    public int Jitter
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(Jitter)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayJitter)));
        }
    } = 0;
    public string DisplayJitter => $"{Jitter} ms";
    public string StandardDeviation
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(StandardDeviation)));
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayStandardDeviation)));
        }
    } = "-";

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
    public bool IsUserDefinedTarget { get; set; } = false;
    public bool FailedPing
    {
        get;
        set
        {
            field = value;
            NotifyPropertyChanged(new PropertyChangedEventArgs(nameof(FailedPing)));
        }
    } = false;
    public DateTime TimeStamp { get; set; }
    
    private void NotifyPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}