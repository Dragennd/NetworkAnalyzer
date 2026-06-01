using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer_UI_Test.ViewModels;
using NetworkAnalyzer_UI_Test.Views;

namespace NetworkAnalyzer_UI_Test.ExtensionMethods;

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
    }
}