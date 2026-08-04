using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Services;
using NetworkAnalyzer.ViewModels;
using NetworkAnalyzer.Views;

namespace NetworkAnalyzer.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection collection)
    {
        // Declare Views
        collection.AddSingleton<HomeView>();
        collection.AddSingleton<IPScannerView>();
        collection.AddSingleton<LatencyMonitorView>();
        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<ReportsView>();
        collection.AddSingleton<SettingsView>();
        
        // Declare View Models
        collection.AddSingleton<HomeViewModel>();
        collection.AddSingleton<IPScannerViewModel>();
        collection.AddSingleton<LatencyMonitorViewModel>();
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<ReportsViewModel>();
        collection.AddSingleton<SettingsViewModel>();
        
        // Process functions and factories
        collection.AddSingleton<IDatabaseHandler, DatabaseHandler>();
        collection.AddTransient<ITracerouteFactory, TracerouteFactory>();
        collection.AddTransient<IDNSHandler, DNSHandler>();
        collection.AddTransient<IMACAddressHandler, MACAddressHandler>();
        collection.AddTransient<IRDPHandler, RDPHandler>();
        collection.AddTransient<ISMBHandler, SMBHandler>();
        collection.AddTransient<ISSHHandler, SSHHandler>();
        collection.AddTransient<ISubnetHandler, SubnetHandler>();
        
        // Service classes
        collection.AddSingleton<LatencyMonitorService>();
        collection.AddSingleton<IPScannerService>();
        collection.AddSingleton<HomeService>();
        
        // Process Controllers
        collection.AddSingleton<IHomeController, HomeController>();
        collection.AddSingleton<ILatencyMonitorController, LatencyMonitorController>();
        collection.AddSingleton<IIPScannerController, IPScannerController>();
        collection.AddSingleton<IReportsController, ReportsController>();
        
        // Global function and property classes
        collection.AddSingleton(resolver => resolver.GetRequiredService<IOptions<GlobalSettings>>().Value);
        collection.AddSingleton<LogHandler>();
        collection.AddSingleton<SocketsHandler>();
    }
}