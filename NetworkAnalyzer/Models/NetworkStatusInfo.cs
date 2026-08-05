using System;
using System.Net.NetworkInformation;
using Avalonia.Media;
using Material.Icons;

namespace NetworkAnalyzer.Models;

public class NetworkStatusInfo
{
    public string TargetName { get; private set; }
    public string TargetLatency { get; private set; }
    public string DisplayLatency { get; private set; }
    public IPStatus Status { get; private set; }
    public IBrush StatusBrush { get; private set; }
    public MaterialIconKind StatusIcon { get; private set; }
    public DateTime Timestamp { get; private set; }

    public NetworkStatusInfo(string targetName, string targetLatency, IPStatus status)
    {
        TargetName = targetName;
        TargetLatency = targetLatency;
        Status = status;
        
        SetColor();
        SetIcon();
        SetDisplayLatency();
        Timestamp = DateTime.Now;
    }

    private void SetColor()
    {
        StatusBrush = Status switch
        {
            IPStatus.Success => Brushes.Green,
            _ => Brushes.Red
        };
    }

    private void SetIcon()
    {
        StatusIcon = Status switch
        {
            IPStatus.Success => MaterialIconKind.CheckCircleOutline,
            _ => MaterialIconKind.CancelCircleOutline
        };
    }

    private void SetDisplayLatency() => 
        DisplayLatency = $" |  {TargetLatency}ms";
}