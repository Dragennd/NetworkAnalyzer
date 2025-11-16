using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.Apps.Home;
using NetworkAnalyzer.Apps.Home.Functions;
using NetworkAnalyzer.Apps.Home.Interfaces;
using NetworkAnalyzer.Apps.IPScanner;
using NetworkAnalyzer.Apps.IPScanner.Functions;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.LatencyMonitor;
using NetworkAnalyzer.Apps.LatencyMonitor.Functions;
using NetworkAnalyzer.Apps.LatencyMonitor.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Reports;
using NetworkAnalyzer.Apps.Reports.Functions;
using NetworkAnalyzer.Apps.Reports.Interfaces;
using NetworkAnalyzer.Apps.Settings;
using NetworkAnalyzer.Apps.Utilities;
using SQLite;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NetworkAnalyzer
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        private GlobalSettings _defaultSettings = new();

        public App()
        {
            ConfirmExistanceOfDirectoryStructure(_defaultSettings);
            ConfirmExistanceOfRequiredFiles(_defaultSettings);

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(_defaultSettings.ConfigDirectory);
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
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<LatencyMonitorViewModel>();                
                    services.AddSingleton<IPScannerViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<ReportsViewModel>();

                    // Process controllers
                    services.AddSingleton<IHomeController, HomeController>();
                    services.AddSingleton<ILatencyMonitorController, LatencyMonitorController>();
                    services.AddSingleton<IIPScannerController, IPScannerController>();
                    services.AddSingleton<IReportsController, ReportsController>();

                    // Process services
                    services.AddSingleton<ILatencyMonitorService, LatencyMonitorService>();
                    services.AddSingleton<IIPScannerService, IPScannerService>();

                    // Utility user controls
                    services.AddSingleton<LatencyMonitorDetailsWindow>();

                    // Utility user control view models
                    services.AddSingleton<LatencyMonitorDetailsWindowViewModel>();

                    // Global function and property classes
                    services.Configure<GlobalSettings>(context.Configuration.GetSection(nameof(GlobalSettings)));
                    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<GlobalSettings>>().Value);
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
            List<DBVersion> version = new();

            // Create the default Settings file
            if (!File.Exists(settings.ConfigPath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalConfigPath))
                {
                    using (FileStream fileStream = new(settings.ConfigPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }

            // Check if Database file exists, if not, create a new file from embedded default
            if (!File.Exists(settings.DatabasePath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalDatabasePath))
                {
                    using (FileStream fileStream = new(settings.DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        stream.CopyTo(fileStream);
                    }
                }

                return;
            }

            try
            {
                using (var dbConnection = new SQLiteConnection(settings.DatabasePath))
                {
                    version = GetDatabaseVersionAsync(dbConnection);
                }

                // If DBVersion table contains an older version, rename to -OLD-v<version number>, then create new Database file
                if (File.Exists(settings.DatabasePath) && version.First().Version != settings.BuildVersion)
                {
                    File.Move(settings.DatabasePath, $@"{settings.ConfigDirectory}\NetworkAnalyzerDB-OLD-v{version.First().Version}.db");

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalDatabasePath))
                    {
                        using (FileStream fileStream = new(settings.DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (SQLiteException)
            {
                // Create the Database file if missing
                if (!File.Exists(settings.DatabasePath))
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalDatabasePath))
                    {
                        using (FileStream fileStream = new(settings.DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
                else
                {
                    // If DBVersion table is empty, rename to -OLD, then create new Database file
                    File.Move(settings.DatabasePath, $@"{settings.ConfigDirectory}\NetworkAnalyzerDB-OLD.db");

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(settings.LocalDatabasePath))
                    {
                        using (FileStream fileStream = new(settings.DatabasePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (IOException)
            {
                DisplayErrorMessage("The database file was inaccessible.\n\n" +
                    "Either the Network Analyzer directory cannot be accessed or the database file is open elsewhere.\n\n" +
                    "Ensure the database file and the Network Analyzer directory are accessible, then try again.\n\n" +
                    "The Network Analyzer application will now close.");

                throw;
            }
        }

        private void DisplayErrorMessage(string message)
        {
            MessageBox.Show(message, "An unexpected error has occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private List<DBVersion> GetDatabaseVersionAsync(SQLiteConnection con) =>
            con.Query<DBVersion>("SELECT Version FROM DBVersion LIMIT 1");
    }
}
