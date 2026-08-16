using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.Services;
using NetworkAnalyzer.Views;

namespace NetworkAnalyzer.ViewModels;

internal partial class MainWindowViewModel : ObservableValidator
{
    public ObservableCollection<NotificationInfo> Notifications { get; private set; } = new();
    
    [ObservableProperty]
    public partial UserControl Content { get; set; }
    
    [ObservableProperty]
    public partial string ContentTitle { get; private set; }

    [ObservableProperty]
    public partial double RowHeight { get; private set; } = 50;

    [ObservableProperty]
    public partial bool IsSubPanelVisible { get; private set; } = false;

    [ObservableProperty]
    public partial bool IsSocketsPopupCardVisible { get; private set; } = false;
    
    public bool IsLatencyMonitorMainMenuButtonChecked
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();

                if (value)
                {
                    OptionsIcon = MaterialIconKind.MenuDownOutline;
                    IsSubPanelVisible = true;
                    RowHeight = 150;
                }
                else
                {
                    OptionsIcon = MaterialIconKind.MenuRightOutline;
                    IsSubPanelVisible = false;
                    RowHeight = 50;
                }
            }
        }
    } = false;

    [ObservableProperty]
    public partial bool IsPkexecUnavailable { get; set; } = true;
    
    [ObservableProperty]
    public partial string CodeToCopy { get; private set; }

    [ObservableProperty]
    public partial MaterialIconKind OptionsIcon { get; set; } = MaterialIconKind.MenuRightOutline;
    public readonly HomeView _home;
    public readonly IPScannerView _ipScanner;
    public readonly LatencyMonitorView _latencyMonitor;
    public readonly ReportsView _reports;
    public readonly SettingsView _settings;
    private readonly SocketsHandler _sockets;
    private string ExecutablePath { get; }
    private readonly MainService _mainService = App.AppHost.Services.GetRequiredService<MainService>();
    private readonly IMainController _mainController;
    
    public MainWindowViewModel(
        HomeView home, 
        IPScannerView ipScanner, 
        LatencyMonitorView latencyMonitor, 
        ReportsView reports, 
        SettingsView settings, 
        SocketsHandler sockets,
        IMainController mainController)
    {
        _home = home;
        _ipScanner = ipScanner;
        _latencyMonitor = latencyMonitor;
        _reports = reports;
        _settings = settings;
        _sockets = sockets;
        _mainController = mainController;

        _mainController.AddNotifications += AddNewNotification;

        Content = _home;
        ContentTitle = "Home";
        ExecutablePath = Environment.ProcessPath!;
        CodeToCopy = $" sudo setcap cap_net_raw+ep \"{ExecutablePath}\"";

        if (OperatingSystem.IsLinux())
        {
            // To-Do: Enable this check prior to final build
            //_ = CheckSystemSocketAccess();   
        }
    }
    
    [RelayCommand]
    public void SetHomeActive()
    {
        Content = _home;
        ContentTitle = "Home";
        IsLatencyMonitorMainMenuButtonChecked = false;
    }
    
    [RelayCommand]
    public void SetIPScannerActive()
    {
        Content = _ipScanner;
        ContentTitle = "IP Scanner";
        IsLatencyMonitorMainMenuButtonChecked = false;
    }
    
    [RelayCommand]
    public void SetLatencyMonitorActive()
    {
        Content = _latencyMonitor;
        ContentTitle = "Latency Monitor";
        IsLatencyMonitorMainMenuButtonChecked = false;
    }
    
    [RelayCommand]
    public void SetReportActive()
    {
        Content = _reports;
        ContentTitle = "Reports";
        IsLatencyMonitorMainMenuButtonChecked = false;
    }
    
    [RelayCommand]
    public void SetSettingsActive()
    {
        Content = _settings;
        ContentTitle = "Settings";
        IsLatencyMonitorMainMenuButtonChecked = false;
    }

    [RelayCommand]
    public async void EnableSockets()
    {
        bool socketsStatus = await _sockets.GrantSocketAccessAsync(ExecutablePath);

        if (!socketsStatus)
        {
            await Dispatcher.UIThread.InvokeAsync(() => 
                DisplayErrorMessage(
                    "Sockets are not enabled", 
                    "Failed to enable sockets for Network Analyzer.\nSee logs in Network Analyzer directory for details."));
        }
        else
        {
            IsSocketsPopupCardVisible = false;
            await RestartNetworkAnalyzer();

            // To-Do: add to toast notification implementation to inform the user when enabling sockets was successful
            // To-Do: Also need to implement toast notification system
        }
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
            IsPkexecUnavailable = !await _sockets.GetPkexecStatus();
        }
    }

    private async Task RestartNetworkAnalyzer()
    {
        try
        {
            // Start a new instance of the Network Analyzer
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                UseShellExecute = true
            };
        
            Process.Start(processStartInfo);
        
            // Close current instance of the Network Analyzer
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
        catch (Exception ex)
        {
            await DisplayErrorMessage(
                "Failed to reload Network Analyzer",
                "Network Analyzer needs to be reloaded to finish applying permission changes.\n Please restart Network Analyzer to continue.");
        }
    }

    private void AddNewNotification(NotificationInfo notification)
    {
        Notifications.Add(notification);
    }
    
    private async Task DisplayErrorMessage(string title, string message)
    {
        await MessageBoxManager
            .GetMessageBoxStandard(
                title, 
                message, 
                ButtonEnum.Ok,
                Icon.Error,
                WindowStartupLocation.CenterScreen
            ).ShowAsync();
    }
}