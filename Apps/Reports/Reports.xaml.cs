using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;


namespace NetworkAnalyzer.Apps.Reports
{
    public partial class Reports : UserControl
    {
        public Reports()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<ReportsViewModel>();
        }
    }
}
