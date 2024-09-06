using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.Reports;
using System.Diagnostics;

namespace NetworkAnalyzer
{
    internal partial class MainWindowViewModel : ObservableValidator
    {
        #region Control Properties
        static private Home Home { get; } = new();
        static private LatencyMonitor LatencyMonitor { get; } = new();
        static private IPScanner IPScanner { get; } = new();
        static private Reports Reports { get; } = new();

        [ObservableProperty]
        public string activeApp = "Home";

        [ObservableProperty]
        public object activeAppInstance = Home;
        #endregion

        public MainWindowViewModel()
        {
            MenuController.activeAppRequest += ProcessActiveAppRequest;
        }

        [RelayCommand]
        public void SetHomeApp()
        {
            MenuController.SendActiveAppRequest("Home");
            ActiveAppInstance = Home;
        }

        [RelayCommand]
        public void SetLatencyMonitorApp()
        {
            MenuController.SendActiveAppRequest("LatencyMonitor");
            ActiveAppInstance = LatencyMonitor;
        }

        [RelayCommand]
        public void SetIPScannerApp()
        {
            MenuController.SendActiveAppRequest("IPScanner");
            ActiveAppInstance = IPScanner;
        }

        [RelayCommand]
        public void SetReportsApp()
        {
            MenuController.SendActiveAppRequest("Reports");
            ActiveAppInstance = Reports;
        }

        [RelayCommand]
        public void SetInfoApp() =>
            Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer?tab=readme-ov-file#networkanalyzer") { UseShellExecute = true });

        #region Private Methods
        private void ProcessActiveAppRequest(string appName)
        {
            ActiveApp = appName;
        }
        #endregion
    }
}
