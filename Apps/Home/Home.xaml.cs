using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.Home
{
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<HomeViewModel>();
        }
    }
}
