using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Enums;
using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Functions;
using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;
using NetworkAnalyzer.Services;

namespace NetworkAnalyzer.ViewModels;

internal partial class IPScannerViewModel : ObservableValidator
{
    #region Properties
    // Contains all results from the network scan
    public ObservableCollection<IPScannerData> AllScanResults { get; set; } = new();

    public IPScannerData? SelectedScanResult
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();

            if (!IsDetailsPaneVisible)
            {
                DataGridRowSpan = 1;
                IsDetailsPaneVisible = true;
            }
        }
    }

    public ScanType ScanType
    {
        get => _ipScannerService.ScanType;
        set
        {
            if (_ipScannerService.ScanType != value)
            {
                _ipScannerService.ScanType = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string CustomPortCollection
    {
        get => _ipScannerService.CustomPortCollection;
        set
        {
            if (_ipScannerService.CustomPortCollection != value)
            {
                _ipScannerService.CustomPortCollection = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string SubnetsToScan
    {
        get => _ipScannerService.SubnetsToScan;
        set
        {
            if (_ipScannerService.SubnetsToScan != value)
            {
                _ipScannerService.SubnetsToScan = value;
                OnPropertyChanged();
            }
        }
    }

    public string ScanDuration
    {
        get => _ipScannerService.ScanDuration;
        set
        {
            if (_ipScannerService.ScanDuration != value)
            {
                _ipScannerService.ScanDuration = value;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    public partial string ScanStatus { get; set; } = "IDLE";

    [ObservableProperty]
    public partial string SubnetsToScanPlaceholderText { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial int TotalAddressCount { get; set; }

    [ObservableProperty]
    public partial int TotalActiveAddresses { get; set; }

    [ObservableProperty]
    public partial int TotalInactiveAddresses { get; set; }

    [ObservableProperty]
    public partial int DataGridRowSpan { get; set; } = 3;

    [ObservableProperty]
    public partial bool IsStartButtonEnabled { get; set; } = true;
    
    [ObservableProperty]
    public partial bool IsStopButtonEnabled { get; set; } = false;

    public bool IsControlFocused
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();

                if (!IsDetailsPaneVisible && value && SelectedScanResult != null)
                {
                    IsDetailsPaneVisible = true;
                    DataGridRowSpan = 1;
                }
                else if (value)
                {
                    IsOptionsButtonChecked = false;
                    IsDetailsPaneVisible = false;
                    DataGridRowSpan = 3;
                }
            }
        }
    } = false;

    public bool IsOptionsButtonChecked
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                IsDetailsPaneVisible = false;
                DataGridRowSpan = 3;

                if (value)
                {
                    OptionsIcon = MaterialIconKind.MenuDownOutline;
                    IsOptionsCardVisible = true;
                }
                else
                {
                    OptionsIcon = MaterialIconKind.MenuRightOutline;
                    IsOptionsCardVisible = false;
                }
            }
        }
    } = false;
    
    public bool IsAutoChecked
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
                    SubnetsToScanPlaceholderText = string.Empty;
                }
                else
                {
                    SubnetsToScanPlaceholderText = "Enter subnet to scan...";
                }
            }
        }
    } = true;

    [ObservableProperty]
    public partial bool IsManualChecked { get; set; } = false;

    [ObservableProperty]
    public partial bool IsCommonScanChecked { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFullScanChecked { get; set; } = false;
    
    [ObservableProperty]
    public partial bool IsCustomScanChecked { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOptionsCardVisible { get; set; } = false;

    [ObservableProperty]
    public partial bool IsDetailsPaneVisible { get; set; } = false;

    [ObservableProperty]
    public partial MaterialIconKind OptionsIcon { get; set; } = MaterialIconKind.MenuRightOutline;
    
    private bool _isScanning;
    private readonly IPScannerController _ipScannerController;
    private readonly IPScannerService _ipScannerService = App.AppHost.Services.GetRequiredService<IPScannerService>();
    private readonly LogHandler _logHandler = App.AppHost.Services.GetRequiredService<LogHandler>();
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();
    #endregion Properties

    public IPScannerViewModel(IPScannerController ipScannerController)
    {
        _ipScannerController = ipScannerController;

        SetScanMode();
    }

    [RelayCommand]
    public async Task StartIPScanButtonAsync()
    {
        try
        {
            ResetStatistics();
            SetSubscriptions();
            _ = StartScanTimer();
            IsOptionsButtonChecked = false;
            IsStartButtonEnabled = false;
            IsStopButtonEnabled = true;

            if (IsCommonScanChecked)
            {
                ScanType = ScanType.Common;
            }
            else if (IsFullScanChecked)
            {
                ScanType = ScanType.Full;
            }
            else if (IsCustomScanChecked)
            {
                ScanType = ScanType.Custom;
            }

            await Task.Run(async () => { await _ipScannerService.StartScanAsync(IsAutoChecked); });
        }
        catch (Exception ex)
        {
            await _logHandler.CreateLogEntry(ex.Message, LogType.Error);
            await _logHandler.CreateLogEntry(ex.StackTrace, LogType.Error);
            _mainController.SendAddNotificationRequest(
                new NotificationInfo(
                    "Failed to Start IP Scan",
                    $"An error occurred while starting the IP scan. See logs for details.",
                    NotificationType.Error));
        }
        finally
        {
            IsStartButtonEnabled = true;
            IsStopButtonEnabled = false;
            UnsetSubscriptions();
        }
    }

    [RelayCommand]
    public void StopIPScanButton() => _ipScannerController.SendSetEmergencyStopStatusRequest(true);

    #region Private Methods
    private void SetSubscriptions()
    {
        _isScanning = true;

        _ipScannerController.AddScanResults += AddScanResults;
        _ipScannerController.UpdateScanStatus += UpdateScanStatus;
        _ipScannerController.UpdateTotalAddressCount += UpdateTotalAddressCount;
        _ipScannerController.UpdateTotalActiveAddresses += UpdateTotalActiveAddresses;
        _ipScannerController.UpdateTotalInactiveAddresses += UpdateTotalInactiveAddresses;
    }

    private void UnsetSubscriptions()
    {
        _isScanning = false;

        _ipScannerController.AddScanResults -= AddScanResults;
        _ipScannerController.UpdateScanStatus -= UpdateScanStatus;
        _ipScannerController.UpdateTotalAddressCount -= UpdateTotalAddressCount;
        _ipScannerController.UpdateTotalActiveAddresses -= UpdateTotalActiveAddresses;
        _ipScannerController.UpdateTotalInactiveAddresses -= UpdateTotalInactiveAddresses;
    }

    private void SetScanMode()
    {
        if (GlobalSettings.DefaultScanMode == "Auto")
        {
            IsAutoChecked = true;
            IsManualChecked = false;
        }
        else if (GlobalSettings.DefaultScanMode == "Manual")
        {
            IsAutoChecked = false;
            IsManualChecked = true;
        }
    }

    private void ResetStatistics()
    {
        AllScanResults.Clear();
        TotalAddressCount = 0;
        TotalActiveAddresses = 0;
        TotalInactiveAddresses = 0;
        ScanDuration = "00:00.000";
    }

    private async Task StartScanTimer()
    {
        Stopwatch sw = Stopwatch.StartNew();

        while (_isScanning)
        {
            ScanDuration = FormatScanDuration(sw.Elapsed);
            await Task.Delay(100);
        }
    }

    private string FormatScanDuration(TimeSpan elapsedTime) =>
        $"{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}";
    
    private async void AddScanResults(IPScannerData data)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            AllScanResults.Add(data);
        });
    }

    private void UpdateScanStatus(string str)
    {
        ScanStatus = str;
    }

    private void UpdateTotalAddressCount(int num)
    {
        TotalAddressCount = num;
    }

    private void UpdateTotalActiveAddresses(int num)
    {
        TotalActiveAddresses = num;
    }

    private void UpdateTotalInactiveAddresses(int num)
    {
        TotalInactiveAddresses = num;
    }
    #endregion Private Methods
}