using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer_UI_Test.Functions;
using NetworkAnalyzer_UI_Test.Views;

namespace NetworkAnalyzer_UI_Test.ViewModels;

internal partial class MainWindowViewModel : ObservableValidator
{
    [ObservableProperty]
    public partial UserControl Content { get; set; }
    
    [ObservableProperty]
    public partial string ContentTitle { get; set; }

    [ObservableProperty]
    public partial double RowHeight { get; set; } = 50;

    [ObservableProperty]
    public partial bool IsSubPanelVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsSocketsPopupCardVisible { get; set; } = false;

    public readonly HomeView _home;
    public readonly IPScannerView _ipScanner;
    public readonly LatencyMonitorView _latencyMonitor;
    public readonly ReportsView _reports;
    public readonly SettingsView _settings;
    private readonly SocketsHandler _sockets;
    
    public MainWindowViewModel(HomeView home, IPScannerView ipScanner, LatencyMonitorView latencyMonitor, ReportsView reports, SettingsView settings, SocketsHandler sockets)
    {
        _home = home;
        _ipScanner = ipScanner;
        _latencyMonitor = latencyMonitor;
        _reports = reports;
        _settings = settings;
        _sockets = sockets;

        Content = _home;
        ContentTitle = "Home";

        _ = CheckSystemSocketAccess();
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
    public void ToggleLatencyMonitorSubPanel()
    {
        IsSubPanelVisible = !IsSubPanelVisible;
        RowHeight = RowHeight == 150 ? 50 : 150;
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

    [RelayCommand]
    public async void EnableSockets()
    {
        bool socketsStatus = await _sockets.GrantSocketAccessAsync();

        if (!socketsStatus)
        {
            await Dispatcher.UIThread.InvokeAsync(() => 
                DisplayErrorMessage("Failed to enable sockets for Network Analyzer.\nSee logs in Network Analyzer directory for details."));
        }
        else
        {
            IsSocketsPopupCardVisible = false;
            // To-Do: add to toast notification implementation to inform the user when enabling sockets was successful
            // To-Do: Also need to implement toast notification system
        }
    }

    [RelayCommand]
    public void CancelSockets()
    {
        // To-Do: Add logic to close the app and end all child processes
    }

    private async Task CheckSystemSocketAccess()
    {
        using var ping = new Ping();

        try
        {
            await ping.SendPingAsync("1.1.1.1", 4000, new byte[32]);
        }
        catch (PlatformNotSupportedException)
        {
            IsSocketsPopupCardVisible = true;
        }
    }
    
    private async Task DisplayErrorMessage(string message)
    {
        await MessageBoxManager
            .GetMessageBoxStandard(
                "Sockets are not enabled", 
                message, 
                ButtonEnum.Ok,
                Icon.Error
            ).ShowAsync();
    }
}