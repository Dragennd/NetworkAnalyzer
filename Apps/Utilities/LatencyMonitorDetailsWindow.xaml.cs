using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace NetworkAnalyzer.Apps.Utilities
{
    public partial class LatencyMonitorDetailsWindow : Window
    {
        public LatencyMonitorDetailsWindow()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetRequiredService<LatencyMonitorDetailsWindowViewModel>();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Do nothing
                // The DragMove() method only supports the primary mouse button
            }
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
