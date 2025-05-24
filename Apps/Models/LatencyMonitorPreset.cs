using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetworkAnalyzer.Apps.Models
{
    public class LatencyMonitorPreset
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ID { get; set; }

        private string presetName = string.Empty;
        public string PresetName
        {
            get
            {
                return presetName;
            }
            set
            {
                presetName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> TargetCollection { get; set; }

        public string UUID { get; set; } = string.Empty;

        public LatencyMonitorPreset([Optional]string defaultUUID)
        {
            if (defaultUUID == "Default")
            {
                UUID = defaultUUID;
            }
            else
            {
                UUID = GenerateNewGUID();
            }

            TargetCollection = new ObservableCollection<string>();
        }

        public string GenerateNewGUID()
        {
            string uuid = Guid.NewGuid().ToString();

            return uuid;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
