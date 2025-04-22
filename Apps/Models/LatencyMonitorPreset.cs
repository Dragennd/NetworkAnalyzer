namespace NetworkAnalyzer.Apps.Models
{
    public class LatencyMonitorPreset
    {
        public List<string> TargetCollection { get; set; }

        public string PresetName { get; set; }

        public LatencyMonitorPreset()
        {
            TargetCollection = new List<string>();
        }
    }
}
