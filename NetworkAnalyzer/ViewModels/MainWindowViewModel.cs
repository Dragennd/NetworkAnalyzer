using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.Services;
using NetworkAnalyzer.Views;

namespace NetworkAnalyzer.ViewModels;

internal partial class MainWindowViewModel : ObservableValidator
{
    public ObservableCollection<NotificationInfo> Notifications { get; set; } = new();
    public ObservableCollection<NotificationInfo> NotificationsQueue { get; set; } = new();
    
    [ObservableProperty]
    public partial UserControl Content { get; set; }
    
    [ObservableProperty]
    public partial NotificationInfo CurrentNotification { get; set; }
    
    [ObservableProperty]
    public partial string ContentTitle { get; private set; }

    [ObservableProperty]
    public partial double RowHeight { get; private set; } = 50;

    [ObservableProperty]
    public partial bool IsSubPanelVisible { get; private set; } = false;

    [ObservableProperty]
    public partial bool IsSocketsPopupCardVisible { get; private set; } = false;

    public bool IsNotificationCenterCardVisible
    { 
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                IsRedDotBadgeVisible = false;
            }
        }
    } = false;
    
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
    public partial bool IsNotificationToastVisible { get; set; } = false;
    
    private bool IsNotificationProcessorActive { get; set; } = false;

    [ObservableProperty]
    public partial bool IsPkexecUnavailable { get; set; } = true;

    [ObservableProperty]
    public partial bool IsRedDotBadgeVisible { get; set; } = false;

    [ObservableProperty]
    public partial string CodeToCopy { get; private set; }

    [ObservableProperty]
    public partial double NotificationProgress { get; set; }

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
    private readonly MainController _mainController;
    
    public MainWindowViewModel(
        HomeView home, 
        IPScannerView ipScanner, 
        LatencyMonitorView latencyMonitor, 
        ReportsView reports, 
        SettingsView settings, 
        SocketsHandler sockets,
        MainController mainController)
    {
        _home = home;
        _ipScanner = ipScanner;
        _latencyMonitor = latencyMonitor;
        _reports = reports;
        _settings = settings;
        _sockets = sockets;
        _mainController = mainController;

        Notifications.CollectionChanged += Notifications_CollectionChanged;
        NotificationsQueue.CollectionChanged += NotificationsQueue_CollectionChanged;
        _mainController.AddNotifications += AddNewNotification;
        _mainController.RemoveNotifications += RemoveNotification;

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
    public void ToggleNotificationsWindow()
    {
        IsNotificationCenterCardVisible = !IsNotificationCenterCardVisible;
    }
    
    [RelayCommand]
    public void RemoveNotification(string guid)
    {
        Notifications.Remove(Notifications.First(a => a.GUID == guid));
    }

    [RelayCommand]
    public void ClearAllNotifications()
    {
        Notifications.Clear();
    }

    [RelayCommand]
    public async void EnableSockets()
    {
        bool socketsStatus = await _sockets.GrantSocketAccessAsync(ExecutablePath);

        if (!socketsStatus)
        {
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Sockets are not enabled",
                    "Failed to enable sockets for Network Analyzer.\nSee logs in Network Analyzer directory for details.",
                    NotificationType.Error));
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
        catch (Exception)
        {
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to reload Network Analyzer", 
                    "Network Analyzer needs to be reloaded to finish applying permission changes.\n Please restart Network Analyzer to continue.", 
                    NotificationType.Error));
        }
    }

    private void AddNewNotification(NotificationInfo notification)
    {
        Notifications.Add(notification);
        NotificationsQueue.Add(notification);
    }

    private void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (Notifications.Count > 0 && !IsNotificationCenterCardVisible)
        {
            IsRedDotBadgeVisible = true;
        }
    }

    private void NotificationsQueue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsNotificationProcessorActive)
            return;

        _ = ProcessIncomingNotificationsAsync();
    }

    private async Task ProcessIncomingNotificationsAsync()
    {
        if (IsNotificationProcessorActive)
            return;
        
        IsNotificationProcessorActive = true;

        try
        {
            while (NotificationsQueue.Count > 0)
            {
                IsNotificationToastVisible = true;
                var notification = NotificationsQueue[0];
                CurrentNotification = notification;
                NotificationProgress = 100;

                const int duration = 5000;
                const int interval = 50;

                for (var elapsed = 0; elapsed < duration; elapsed += interval)
                {
                    await Task.Delay(interval);

                    NotificationProgress = 100.0 - ((double)elapsed / duration * 100.0);
                }

                if (NotificationsQueue.Count > 0 && NotificationsQueue[0] == notification)
                {
                    NotificationsQueue.RemoveAt(0);
                }
                
                CurrentNotification = null;
                NotificationProgress = 0;
                IsNotificationToastVisible = false;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }
        finally
        {
            IsNotificationProcessorActive = false;
        }
    }
}