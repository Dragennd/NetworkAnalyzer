using System;
using System.Collections.ObjectModel;
using SQLite;

namespace NetworkAnalyzer.Models;

public class LatencyMonitorPreset
{
    public int ID { get; set; }
        
    public string PresetName { get; set; }
        
    public ObservableCollection<string> TargetCollection { get; set; }

    public string UUID { get; set; } = string.Empty;

    public LatencyMonitorPreset(bool isNew = false)
    {
        if (isNew)
        {
            UUID = GenerateNewGUID();
        }
            
        TargetCollection = new();
    }

    private string GenerateNewGUID() => Guid.NewGuid().ToString();
}

[Table("LatencyMonitorTargetProfiles")]
internal class LatencyMonitorTargetProfiles
{
    [PrimaryKey, AutoIncrement]
    [Column("ID")]
    public int ID { get; set; }

    [Column("ProfileName")]
    public string ProfileName { get; set; }

    [Column("TargetCollection")]
    public string TargetCollection { get; set; }

    [Column("UUID")]
    public string UUID { get; set; }
}