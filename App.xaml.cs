using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.Reports;
using NetworkAnalyzer.Apps.Settings;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using static NetworkAnalyzer.Apps.GlobalClasses.DataStore;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Reports.Functions;

namespace NetworkAnalyzer
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<Home>();
                services.AddSingleton<LatencyMonitor>();
                services.AddSingleton<LatencyMonitorViewModel>();
                services.AddSingleton<IPScanner>();
                services.AddSingleton<IPScannerViewModel>();
                services.AddSingleton<Reports>();
                services.AddSingleton<Settings>();
                services.AddSingleton<ILatencyMonitorService, LatencyMonitorService>();
                services.AddSingleton<ITracerouteFactory, TracerouteFactory>();
                services.AddSingleton<ILatencyMonitorController, LatencyMonitorController>();
                services.AddSingleton<IDatabaseHandler, DatabaseHandler>();
            }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }

            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            if (!Directory.Exists(ReportDirectory))
            {
                Directory.CreateDirectory(ReportDirectory);
            }

            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();

            base.OnExit(e);
        }

        //public void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    if (!Directory.Exists(DataDirectory))
        //    {
        //        Directory.CreateDirectory(DataDirectory);
        //    }

        //    if (!Directory.Exists(ConfigDirectory))
        //    {
        //        Directory.CreateDirectory(ConfigDirectory);
        //    }

        //    if (!Directory.Exists(ReportDirectory))
        //    {
        //        Directory.CreateDirectory(ReportDirectory);
        //    }

        //    if (!Directory.Exists(LogDirectory))
        //    {
        //        Directory.CreateDirectory(LogDirectory);
        //    }
        //}
    }
}
