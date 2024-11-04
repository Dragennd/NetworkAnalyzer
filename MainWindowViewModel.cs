using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.GlobalClasses;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.Reports;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;

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
            MenuController.ActiveAppRequest += ProcessActiveAppRequest;

            GenerateDatabase();
        }

        [RelayCommand]
        public void SetHomeApp()
        {
            MenuController.SendActiveAppRequest("Home");
        }

        [RelayCommand]
        public void SetLatencyMonitorApp()
        {
            MenuController.SendActiveAppRequest("LatencyMonitor");
        }

        [RelayCommand]
        public void SetIPScannerApp()
        {
            MenuController.SendActiveAppRequest("IPScanner");
        }

        [RelayCommand]
        public void SetReportsApp() =>
            MenuController.SendActiveAppRequest("Reports");

        [RelayCommand]
        public void SetInfoApp() =>
            Process.Start(new ProcessStartInfo("https://github.com/Dragennd/NetworkAnalyzer/wiki") { UseShellExecute = true });

        #region Private Methods
        private void ProcessActiveAppRequest(string appName)
        {
            ActiveApp = appName;

            switch (ActiveApp)
            {
                case "Home":
                    ActiveAppInstance = Home;
                    break;
                case "LatencyMonitor":
                    ActiveAppInstance = LatencyMonitor;
                    break;
                case "IPScanner":
                    ActiveAppInstance = IPScanner;
                    break;
                case "Reports":
                    ActiveAppInstance = Reports;
                    break;
            }
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
        #endregion
    }
}
