using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.Reports;
using NetworkAnalyzer.Apps.Settings;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        static private Home HomeControl = new();
        static private LatencyMonitor LatencyMonitorControl = new();
        static private IPScanner IPScannerControl = new();
        static private Reports ReportsControl = new();
        static private Settings SettingsControl = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindowForm_Loaded(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = HomeControl;
            BtnHome.IsChecked = true;
            HomeConnectionUtility.SendUpdateChangelogRequest();
            GenerateDatabase();
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

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HomeButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = HomeControl;
        }

        private void LatencyMonitorButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = LatencyMonitorControl;
        }

        private void IPScannerButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = IPScannerControl;
        }

        private void ReportsButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = ReportsControl;
        }

        private void SettingsButton_Checked(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = SettingsControl;
        }

        private void GenerateDatabase()
        {
            if (!File.Exists(DatabasePath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(LocalDatabasePath))
                {
                    using (FileStream fileStream = new FileStream(DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}