using System.Windows.Controls;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.Reports
{
    public partial class Reports : UserControl
    {
        public Reports()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ReportsViewModel viewModel = (ReportsViewModel)DataContext;
            await viewModel.GetReportDirectoryContentsAsync();
            await MonitorReportAvailability();
        }

        private async Task MonitorReportAvailability()
        {
            ReportsViewModel viewModel = (ReportsViewModel)DataContext;

            while (ReportSessionData.IsEmpty)
            {
                await Task.Delay(1000);
            }

            viewModel.IsRBLatencyMonitorChecked = false;
        }
    }
}
