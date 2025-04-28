using System.Collections.ObjectModel;

namespace NetworkAnalyzer.Apps.Models
{
    public class LatencyMonitorPreset
    {
        public int ID { get; set; }

        public string PresetName { get; set; }

        public ObservableCollection<string> TargetCollection { get; set; }

        public string UUID { get; set; }

        public LatencyMonitorPreset()
        {
            TargetCollection = new ObservableCollection<string>();
            UUID = GenerateNewGUID();
        }

        public string GenerateNewGUID()
        {
            string uuid = Guid.NewGuid().ToString();

            return uuid;
        }
    }
}
