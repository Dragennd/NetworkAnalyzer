using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace NetworkAnalyzer.Apps.Settings
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
        }
    }
}
