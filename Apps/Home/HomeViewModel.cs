using CommunityToolkit.Mvvm.ComponentModel;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Home
{
    public partial class HomeViewModel : ObservableRecipient
    {
        [ObservableProperty]
        public string buildID = CurrentBuild;
    }
}
