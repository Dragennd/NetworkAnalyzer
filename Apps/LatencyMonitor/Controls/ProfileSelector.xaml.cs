using System.Windows;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.LatencyMonitor.Controls
{
    public partial class ProfileSelector : UserControl
    {
        public ProfileSelector()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ProfileSelectorViewModel viewModel = (ProfileSelectorViewModel)DataContext;
            await viewModel.LoadTargetProfilesAsync();
        }
    }
}
