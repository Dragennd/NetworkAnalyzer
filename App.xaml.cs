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
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.Utilities;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;

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
                services.AddSingleton<LatencyMonitorDetailsWindow>();
                services.AddSingleton<LatencyMonitorDetailsWindowViewModel>();
                services.AddSingleton<GlobalSettings>();
                services.AddSingleton<LogHandler>();
                services.AddSingleton<ILatencyMonitorService, LatencyMonitorService>();
                services.AddSingleton<ITracerouteFactory, TracerouteFactory>();
                services.AddSingleton<ILatencyMonitorController, LatencyMonitorController>();
                services.AddSingleton<IDatabaseHandler, DatabaseHandler>();
                services.AddSingleton<ISSHHandler, SSHHandler>();
                services.AddSingleton<ISMBHandler, SMBHandler>();
                services.AddSingleton<IRDPHandler, RDPHandler>();
                services.AddSingleton<IDNSHandler, DNSHandler>();
            }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var globalSettings = AppHost.Services.GetRequiredService<GlobalSettings>();
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            if (!Directory.Exists(globalSettings.DataDirectory))
            {
                Directory.CreateDirectory(globalSettings.DataDirectory);
            }

            if (!Directory.Exists(globalSettings.ConfigDirectory))
            {
                Directory.CreateDirectory(globalSettings.ConfigDirectory);
            }

            if (!Directory.Exists(globalSettings.ReportDirectory))
            {
                Directory.CreateDirectory(globalSettings.ReportDirectory);
            }

            if (!Directory.Exists(globalSettings.LogDirectory))
            {
                Directory.CreateDirectory(globalSettings.LogDirectory);
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();

            base.OnExit(e);
        }
    }
}
