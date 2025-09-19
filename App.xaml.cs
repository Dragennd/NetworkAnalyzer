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
                // Top level user controls
                services.AddSingleton<MainWindow>();
                services.AddSingleton<Home>();
                services.AddSingleton<LatencyMonitor>();
                services.AddSingleton<IPScanner>();
                services.AddSingleton<Reports>();
                services.AddSingleton<Settings>();

                // Top level user control view models
                services.AddSingleton<LatencyMonitorViewModel>();                
                services.AddSingleton<IPScannerViewModel>();

                // Process controllers
                services.AddSingleton<ILatencyMonitorController, LatencyMonitorController>();
                services.AddSingleton<IIPScannerController, IPScannerController>();

                // Process services
                services.AddSingleton<ILatencyMonitorService, LatencyMonitorService>();
                services.AddSingleton<IIPScannerService, IPScannerService>();

                // Utility user controls
                services.AddSingleton<LatencyMonitorDetailsWindow>();

                // Utility user control view models
                services.AddSingleton<LatencyMonitorDetailsWindowViewModel>();

                // Global function and property classes
                services.AddSingleton<GlobalSettings>();
                services.AddSingleton<LogHandler>();

                // Process functions and factories
                services.AddSingleton<ITracerouteFactory, TracerouteFactory>();
                services.AddSingleton<IDatabaseHandler, DatabaseHandler>();
                services.AddTransient<ISSHHandler, SSHHandler>();
                services.AddTransient<ISMBHandler, SMBHandler>();
                services.AddTransient<IRDPHandler, RDPHandler>();
                services.AddTransient<IDNSHandler, DNSHandler>();
                services.AddTransient<ISubnetHandler, SubnetHandler>();
                services.AddTransient<IMACAddressHandler, MACAddressHandler>();
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
