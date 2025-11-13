using SQLite;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetworkAnalyzer.Apps.Models
{
    public class LatencyMonitorPreset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ID { get; set; }

        private string presetName = string.Empty;
        public string PresetName
        {
            get => presetName;
            set
            {
                presetName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> targetCollection;
        public ObservableCollection<string> TargetCollection
        {
            get => targetCollection;
            set
            {
                targetCollection = value;
                OnPropertyChanged();
            }
        }

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

            targetCollection = new ObservableCollection<string>();
            targetCollection.CollectionChanged += TargetCollection_CollectionChanged;
        }

        private string GenerateNewGUID() => Guid.NewGuid().ToString();

        private void TargetCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TargetCollection));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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
}
