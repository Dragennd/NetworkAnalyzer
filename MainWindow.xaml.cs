using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.Reports;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        //private bool HomeButtonSelected { get; set; } = false;
        //private bool LatencyMonitorButtonSelected { get; set; } = false;
        //private bool IPScannerButtonSelected { get; set; } = false;
        //private bool InfoButtonSelected { get; set; } = false;
        //private bool ReportsButtonSelected { get; set; } = false;

        //static readonly private Home Home = new();
        //static readonly private LatencyMonitor LatencyMonitor = new();
        //static readonly private IPScanner IPScanner = new();
        //static readonly private Reports Reports = new();

        // Window Controls
        public MainWindow()
        {
            InitializeComponent();
            SizeToContent = SizeToContent.Manual;

            //HomeButtonSelected = true;
            //AppContentControl.Content = Home;
            //UpdateActiveApp();

            VerifyAppDirectoriesExist();
        }

        public void TbtnBase_Checked(object sender, RoutedEventArgs e)
        {
            var darkModeDictionary = new ResourceDictionary();
            var lightModeDictionary = new ResourceDictionary();

            darkModeDictionary.Source = new Uri("Styles/DarkModeTheme.xaml", UriKind.RelativeOrAbsolute);
            lightModeDictionary.Source = new Uri("Styles/LightModeTheme.xaml", UriKind.RelativeOrAbsolute);

            DockPanel.SetDock(TbtnSlider, Dock.Right);
            Application.Current.Resources.MergedDictionaries.Add(lightModeDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(darkModeDictionary);
        }

        public void TbtnBase_Unchecked(object sender, RoutedEventArgs e)
        {
            var darkModeDictionary = new ResourceDictionary();
            var lightModeDictionary = new ResourceDictionary();

            darkModeDictionary.Source = new Uri("Styles/DarkModeTheme.xaml", UriKind.RelativeOrAbsolute);
            lightModeDictionary.Source = new Uri("Styles/LightModeTheme.xaml", UriKind.RelativeOrAbsolute);

            DockPanel.SetDock(TbtnSlider, Dock.Left);
            Application.Current.Resources.MergedDictionaries.Add(darkModeDictionary);
            Application.Current.Resources.MergedDictionaries.Remove(lightModeDictionary);
        }

        // Menu Controls
        //private void BtnHome_Click(object sender, RoutedEventArgs e)
        //{
        //    AppContentControl.Content = Home;
        //    MenuController.SendActiveAppRequest("Home");
        //}

        //private void BtnLatencyMonitor_Click(object sender, RoutedEventArgs e)
        //{
        //    AppContentControl.Content = LatencyMonitor;
        //    MenuController.SendActiveAppRequest("Latency Monitor");
        //}

        //private void BtnIPScanner_Click(object sender, RoutedEventArgs e)
        //{
        //    AppContentControl.Content = IPScanner;
        //    MenuController.SendActiveAppRequest("IP Scanner");
        //}

        //private void BtnReports_Click(object sender, RoutedEventArgs e)
        //{
        //    AppContentControl.Content = Reports;
        //    MenuController.SendActiveAppRequest("Reports");
        //}

        //private void BtnInfo_Click(object sender, RoutedEventArgs e) =>
        //    Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer?tab=readme-ov-file#networkanalyzer") { UseShellExecute = true });

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            GridCloseOptions.Visibility = Visibility.Visible;
            GridCloseOptionsBG.Visibility = Visibility.Visible;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        //private void UpdateActiveApp()
        //{
        //    if (HomeButtonSelected)
        //    {
        //        BtnHome.Style = FindResource("SelectedButtonTemplate") as Style;
        //        BtnLatencyMonitor.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnIPScanner.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnReports.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
        //    }
        //    else if (LatencyMonitorButtonSelected)
        //    {
        //        BtnHome.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnLatencyMonitor.Style = FindResource("SelectedButtonTemplate") as Style;
        //        BtnIPScanner.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnReports.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
        //    }
        //    else if (IPScannerButtonSelected)
        //    {
        //        BtnHome.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnLatencyMonitor.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnIPScanner.Style = FindResource("SelectedButtonTemplate") as Style;
        //        BtnReports.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
        //    }
        //    else if (ReportsButtonSelected)
        //    {
        //        BtnHome.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnLatencyMonitor.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnIPScanner.Style = FindResource("MenuButtonTemplate") as Style;
        //        BtnReports.Style = FindResource("SelectedButtonTemplate") as Style;
        //        BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
        //    }
        //}

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TBIcon.Dispose();
        }

        private void MinimizeToSystemTray_Click(object sender, RoutedEventArgs e)
        {
            MainForm.Visibility = Visibility.Hidden;
            GridCloseOptions.Visibility = Visibility.Hidden;
            GridCloseOptionsBG.Visibility = Visibility.Hidden;
        }

        private void CancelClose_Click(object sender, RoutedEventArgs e)
        {
            GridCloseOptions.Visibility = Visibility.Hidden;
            GridCloseOptionsBG.Visibility = Visibility.Hidden;
        }

        private void ShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainForm.Visibility = Visibility.Visible;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            TBIcon.Dispose();
        }

        private void VerifyAppDirectoriesExist()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            if (!Directory.Exists(ReportDirectory))
            {
                Directory.CreateDirectory(ReportDirectory);
            }
        }
    }
}