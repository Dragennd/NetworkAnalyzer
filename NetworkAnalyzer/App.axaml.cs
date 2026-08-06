using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LiveChartsCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer.ExtensionMethods;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.ViewModels;
using SQLite;

namespace NetworkAnalyzer;

public partial class App : Application
{
    public static IHost AppHost { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ConfirmExistanceOfDirectoryStructure();
        ConfirmExistanceOfRequiredFiles();
        
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(GlobalSettings.ConfigDirectory);
                config.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<GlobalSettings>(context.Configuration.GetSection(nameof(GlobalSettings)));
                services.RegisterServices();
            }).Build();
        
        LiveCharts.Configure(config =>
            config
                .HasMap<NetworkStatusInfo>((info, position) => new(info.Timestamp.ToOADate(), info.TargetLatency))
                .HasMap<LatencyMonitorData>((info, position) =>
                {
                    double.TryParse(info.Latency, out var latency);
                    return new(info.TimeStamp.ToOADate(), latency);
                }));
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = AppHost.Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void ConfirmExistanceOfDirectoryStructure()
    {
        if (!Directory.Exists(GlobalSettings.AppDirectory))
        {
            Directory.CreateDirectory(GlobalSettings.AppDirectory);
        }

        if (!Directory.Exists(GlobalSettings.ConfigDirectory))
        {
            Directory.CreateDirectory(GlobalSettings.ConfigDirectory);
        }

        if (!Directory.Exists(GlobalSettings.ReportDirectory))
        {
            Directory.CreateDirectory(GlobalSettings.ReportDirectory);
        }

        if (!Directory.Exists(GlobalSettings.LogDirectory))
        {
            Directory.CreateDirectory(GlobalSettings.LogDirectory);
        }
    }

    private void ConfirmExistanceOfRequiredFiles()
    {
        List<DBVersion> version = new();

        // Check if Config file exists, if not, create the default Settings file
        if (!File.Exists(GlobalSettings.ConfigPath))
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GlobalSettings.LocalConfigPath);
            using FileStream fileStream = File.Create(GlobalSettings.ConfigPath);
            
            stream.CopyTo(fileStream);
        }

        // Check if Database file exists, if not, create a new file from embedded default
        if (!File.Exists(GlobalSettings.DatabasePath))
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GlobalSettings.LocalDatabasePath);
            using FileStream fileStream = File.Create(GlobalSettings.DatabasePath);
            
            stream.CopyTo(fileStream);

            return;
        }

        try
        {
            using var dbConnection = new SQLiteConnection(GlobalSettings.DatabasePath);
            version = GetDatabaseVersion(dbConnection);

            // If DBVersion table contains an older version, rename to -OLD-v<version number>, then create new Database file
            if (File.Exists(GlobalSettings.DatabasePath) && version.First().Version != GlobalSettings.BuildVersion)
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GlobalSettings.LocalDatabasePath);
                using FileStream fileStream = File.Create(GlobalSettings.DatabasePath);
                
                File.Move(GlobalSettings.DatabasePath, Path.Combine(GlobalSettings.ConfigDirectory, $"NetworkAnalyzerDB-OLD-v{version.First().Version}.db"));
                
                stream.CopyTo(fileStream);
            }
        }
        catch (SQLiteException)
        {
            // Create the Database file if missing
            if (!File.Exists(GlobalSettings.DatabasePath))
            {
                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GlobalSettings.LocalDatabasePath);
                using FileStream fileStream = File.Create(GlobalSettings.DatabasePath);
                
                stream.CopyTo(fileStream);
            }
            else
            {
                // If DBVersion table is empty, rename to -OLD, then create new Database file
                File.Move(GlobalSettings.DatabasePath, Path.Combine(GlobalSettings.ConfigDirectory, "NetworkAnalyzerDB-OLD.db"));

                using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GlobalSettings.LocalDatabasePath);
                using FileStream fileStream = File.Create(GlobalSettings.DatabasePath);
                
                stream.CopyTo(fileStream);
            }
        }
        catch (IOException)
        {
            Dispatcher.UIThread.Invoke(() => 
                DisplayErrorMessage(
                    "The database file was inaccessible.\n\n" +
                    "Either the Network Analyzer directory cannot be accessed or the database file is open elsewhere.\n\n" +
                    "Ensure the database file and the Network Analyzer directory are accessible, then try again.\n\n" +
                    "The Network Analyzer application will now close."));

            throw;
        }
    }

    private async Task DisplayErrorMessage(string message)
    {
        await MessageBoxManager
            .GetMessageBoxStandard(
                "An unexpected error has occurred", 
                message, 
                ButtonEnum.Ok,
                Icon.Error
            ).ShowAsync();
    }

    private List<DBVersion> GetDatabaseVersion(SQLiteConnection con) =>
        con.Query<DBVersion>("SELECT Version FROM DBVersion LIMIT 1");
}