using System.Windows.Controls;

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
        }
    }
}
