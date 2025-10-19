using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Reports;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using NetworkAnalyzer.Apps.Utilities;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NetworkAnalyzer
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            var defaults = new GlobalSettings();

            ConfirmExistanceOfDirectoryStructure(defaults);

            ConfirmExistanceOfRequiredFiles(defaults);

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(defaults.ConfigDirectory);
                    config.AddJsonFile("config.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
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
                    services.AddSingleton<SettingsViewModel>();

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
                    services.Configure<GlobalSettings>(context.Configuration.GetSection(nameof(GlobalSettings)));
                    services.AddSingleton(resolver => resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<GlobalSettings>>().Value);
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

                    // Global Settings
                
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            var settings = AppHost.Services.GetRequiredService<IOptions<GlobalSettings>>().Value;

            settings.UpdateActiveTheme();

            mainWindow.Show();

            base.OnStartup(e);

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();

            base.OnExit(e);
        }

        private void ConfirmExistanceOfDirectoryStructure(GlobalSettings settings)
        {
            if (!Directory.Exists(settings.AppDirectory))
            {
                Directory.CreateDirectory(settings.AppDirectory);
            }

            if (!Directory.Exists(settings.ConfigDirectory))
            {
                Directory.CreateDirectory(settings.ConfigDirectory);
            }

            if (!Directory.Exists(settings.ReportDirectory))
            {
                Directory.CreateDirectory(settings.ReportDirectory);
            }

            if (!Directory.Exists(settings.LogDirectory))
            {
                Directory.CreateDirectory(settings.LogDirectory);
            }
        }

        private void ConfirmExistanceOfRequiredFiles(GlobalSettings settings)
        {
            if (!File.Exists(settings.DatabasePath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalDatabasePath))
                {
                    using (FileStream fileStream = new FileStream(settings.DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            if (!File.Exists(settings.ConfigPath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalConfigPath))
                {
                    using (FileStream fileStream = new FileStream(settings.ConfigPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
