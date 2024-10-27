using System.Windows;
using System.Windows.Controls;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public partial class LatencyMonitor : UserControl
    {
        public LatencyMonitor()
        {
            InitializeComponent();

            // Assign the Target Name fields to the MouseEnter event so their DNS Host Entry is dynamically updated
            TxtIPInfo1D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo2D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo3D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo4D.MouseEnter += TargetName_MouseEnter;
            TxtIPInfo5D.MouseEnter += TargetName_MouseEnter;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LatencyMonitorViewModel viewModel = (LatencyMonitorViewModel)DataContext;
            await viewModel.LoadTargetProfilesAsync();
        }

        // Generate a tooltip containing the DNS Host Entry
        private void TargetName_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!LiveSessionData.IsEmpty)
            {
                TextBox textBox = (TextBox)sender;

                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    string ipAddress = LiveSessionData[textBox.Text].Last().DNSHostName;

                    ToolTip toolTip = new()
                    {
                        Style = (Style)FindResource("InfoToolTip"),
                        Content = $"Resolved IP Address\n{ipAddress}"
                    };

                    ToolTipService.SetToolTip(textBox, toolTip);
                }
            }
        }
    }
}
