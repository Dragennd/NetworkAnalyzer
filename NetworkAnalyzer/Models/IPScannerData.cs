using System.Collections.Generic;
using NetworkAnalyzer.Enums;

namespace NetworkAnalyzer.Models;

internal class IPScannerData
{
    public int ID { get; set; }
    public string ReportID { get; set; }
    public string? Name { get; set; }
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? Manufacturer { get; set; }
    public long? Latency { get; set; }
    public string? DisplayLatency
    {
        get
        {
            if (Latency >= 1)
            {
                return $"{Latency} ms";
            }
            
            if (Latency < 1)
            {
                return "<1 ms";
            }

            return DisplayLatency;
        }
    }
    public List<ActiveService> ActiveServices { get; set; } = new();
    public List<string>? ActivePorts { get; set; } = new();
}