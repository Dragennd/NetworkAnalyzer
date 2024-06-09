using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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

        private void BtnClose_Click(object sender, RoutedEventArgs e) => 
            Close();

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => 
            WindowState = WindowState.Minimized;


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

        private void BtnInfo_Click(object sender, RoutedEventArgs e) => 
            Process.Start(new ProcessStartInfo("https://bitwizards.itglue.com/1518377/docs/14524398") { UseShellExecute = true });

        public void UpdateActiveApp()
        {
            if (HomeButtonSelected)
            {
                BtnHome.Style = FindResource("SelectedButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("MenuButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("MenuButtonTemplate") as Style;
                BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
            }
            else if (LatencyMonitorButtonSelected)
            {
                BtnHome.Style = FindResource("MenuButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("SelectedButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("MenuButtonTemplate") as Style;
                BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
            }
            else
            {
                BtnHome.Style = FindResource("MenuButtonTemplate") as Style;
                BtnLatencyMonitor.Style = FindResource("MenuButtonTemplate") as Style;
                BtnIPScanner.Style = FindResource("SelectedButtonTemplate") as Style;
                BtnInfo.Style = FindResource("MenuButtonTemplate") as Style;
            }
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
    }
}