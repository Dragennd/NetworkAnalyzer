using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.Home.Interfaces;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.Reports;
using NetworkAnalyzer.Apps.Settings;
using System.Windows;
using System.Windows.Input;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        private readonly IHomeController _homeController = App.AppHost.Services.GetRequiredService<IHomeController>();

        private readonly GlobalSettings _settings = App.AppHost.Services.GetRequiredService<GlobalSettings>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindowForm_Loaded(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<Home>();
            BtnHome.IsChecked = true;
            _homeController.SendUpdateChangelogRequest();
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

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (_settings.DefaultAppCloseBehavior == "Close")
            {
                Close();
                TBIcon.Dispose();
            }
            else if (_settings.DefaultAppCloseBehavior == "Minimize")
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void HomeButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<Home>();
        }

        private void LatencyMonitorButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<LatencyMonitor>();
        }

        private void IPScannerButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<IPScanner>();
        }

        private void ReportsButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<Reports>();
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = App.AppHost.Services.GetRequiredService<Settings>();
        }

        private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TBIcon.Dispose();
        }
    }
}