using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        public bool HomeButtonSelected { get; set; } = false;
        public bool LatencyMonitorButtonSelected { get; set; } = false;
        public bool IPScannerButtonSelected { get; set; } = false;
        public bool InfoButtonSelected { get; set; } = false;

        static public Home Home = new();
        static public LatencyMonitor LatencyMonitor = new();
        static public IPScanner IPScanner = new();

        // Window Controls
        public MainWindow()
        {
            InitializeComponent();
            SizeToContent = SizeToContent.Manual;

            HomeButtonSelected = true;
            AppContentControl.Content = Home;
            UpdateActiveApp();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (LatencyMonitor.IsRunning)
            {
                var confirm = MessageBox.Show("The Latency Monitor is currently running, are you sure you want to close the MITS Network Analyzer?",
                                              "Application Close Confirmation",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Exclamation,
                                              MessageBoxResult.No);
                                              
                if (confirm == MessageBoxResult.Yes)
                {
                    Close();
                }
                else
                {
                    return;
                }
            }
            else
            {
                Close();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        // Menu Controls
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            AppContentControl.Content = Home;

            HomeButtonSelected = true;
            LatencyMonitorButtonSelected = false;
            IPScannerButtonSelected = false;

            UpdateActiveApp();
        }

        private void BtnLatencyMonitor_Click(object sender, RoutedEventArgs e)
        {
            AppContentControl.Content = LatencyMonitor;

            HomeButtonSelected = false;
            LatencyMonitorButtonSelected = true;
            IPScannerButtonSelected = false;

            UpdateActiveApp();
        }

        private void BtnIPScanner_Click(object sender, RoutedEventArgs e)
        {
            AppContentControl.Content = IPScanner;

            HomeButtonSelected = false;
            LatencyMonitorButtonSelected = false;
            IPScannerButtonSelected = true;

            UpdateActiveApp();
        }

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://bitwizards.itglue.com/1518377/docs/14524398") { UseShellExecute = true });
        }

        public void UpdateActiveApp()
        {
            if (HomeButtonSelected)
            {
                BtnHome.Style = FindResource("DarkModeSelectedButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnInfo.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
            }
            else if (LatencyMonitorButtonSelected)
            {
                BtnHome.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("DarkModeSelectedButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnInfo.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
            }
            else
            {
                BtnHome.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("DarkModeSelectedButtonTemplate") as Style;
                BtnInfo.Style = FindResource("DarkModeMenuButtonTemplate") as Style;
            }
        }
    }
}