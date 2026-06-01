using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer_UI_Test.Views;

namespace NetworkAnalyzer_UI_Test.ViewModels;

internal partial class MainWindowViewModel : ObservableValidator
{
    [ObservableProperty]
    public partial UserControl Content { get; set; }
    
    [ObservableProperty]
    public partial string ContentTitle { get; set; }

    public readonly HomeView _home;
    public readonly IPScannerView _ipScanner;
    public readonly LatencyMonitorView _latencyMonitor;
    public readonly ReportsView _reports;
    public readonly SettingsView _settings;
    
    public MainWindowViewModel(HomeView home, IPScannerView ipScanner, LatencyMonitorView latencyMonitor, ReportsView reports, SettingsView settings)
    {
        _home = home;
        _ipScanner = ipScanner;
        _latencyMonitor = latencyMonitor;
        _reports = reports;
        _settings = settings;

        Content = _home;
        ContentTitle = "Home";
    }
    
    [RelayCommand]
    public void SetHomeActive()
    {
        Content = _home;
        ContentTitle = "Home";
    }
    
    [RelayCommand]
    public void SetIPScannerActive()
    {
        Content = _ipScanner;
        ContentTitle = "IP Scanner";
    }
    
    [RelayCommand]
    public void SetLatencyMonitorActive()
    {
        Content = _latencyMonitor;
        ContentTitle = "Latency Monitor";
    }
    
    [RelayCommand]
    public void SetReportActive()
    {
        Content = _reports;
        ContentTitle = "Reports";
    }
    
    [RelayCommand]
    public void SetSettingsActive()
    {
        Content = _settings;
        ContentTitle = "Settings";
    }
}