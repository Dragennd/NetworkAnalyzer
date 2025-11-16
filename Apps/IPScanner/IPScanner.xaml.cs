using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.IPScanner
{
    public partial class IPScanner : UserControl
    {
        public IPScanner()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<IPScannerViewModel>();
        }
    }
}
