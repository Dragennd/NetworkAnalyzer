using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.LatencyMonitor
{
    public partial class LatencyMonitor : UserControl
    {
        public LatencyMonitor()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<LatencyMonitorViewModel>();
        }
    }
}
