using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using NetworkAnalyzer.Apps.Settings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal partial class IPScannerViewModel : ObservableValidator, INotifyPropertyChanged
    {
        #region Properties
        // Contains all results from the network scan
        public ObservableCollection<IPScannerData> AllScanResults { get; set; }

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
        public string scanStatus = "IDLE";

        [ObservableProperty]
        public int totalAddressCount;

        [ObservableProperty]
        public int totalActiveAddresses;

        [ObservableProperty]
        public int totalInactiveAddresses;

        [ObservableProperty]
        public bool isStartButtonEnabled = true;

        [ObservableProperty]
        public bool isAutoChecked = true;

        [ObservableProperty]
        public bool isManualChecked = false;

        private bool _isScanning = false;

        private readonly IIPScannerService _ipScannerService;

        private readonly IIPScannerController _ipScannerController;

        private readonly GlobalSettings _settings;

        private readonly IRDPHandler _rdp;

        private readonly ISSHHandler _ssh;

        private readonly ISMBHandler _smb;

        private readonly LogHandler _logHandler;
        #endregion Properties

        public IPScannerViewModel(IIPScannerService ipScannerService, IIPScannerController ipScannerController, GlobalSettings settings, LogHandler logHandler, IRDPHandler rdpHandler, ISSHHandler SSHHandler, ISMBHandler smbHandler)
        {
            AllScanResults = new();
            _ipScannerService = ipScannerService;
            _ipScannerController = ipScannerController;
            _settings = settings;
            _logHandler = logHandler;
            _rdp = rdpHandler;
            _ssh = SSHHandler;
            _smb = smbHandler;

            SetScanMode();
        }

        [RelayCommand]
        public async Task StartIPScanButtonAsync()
        {
            try
            {
                ResetStatistics();
                SetSubscriptions();
                StartScanTimer();
                IsStartButtonEnabled = false;
                await _ipScannerService.StartScanAsync(IsAutoChecked);
                IsStartButtonEnabled = true;
                UnsetSubscriptions();
            }
            catch (Exception ex)
            {
                await _logHandler.CreateLogEntry(ex.ToString(), LogType.Error, ReportType.ICMP);
            }
        }

        [RelayCommand]
        public async Task ConnectRDPButtonAsync(string ipAddress) => await _rdp.StartRDPSessionAsync(ipAddress);

        [RelayCommand]
        public async Task ConnectSSHButtonAsync(string ipAddress) => await _ssh.StartSSHSessionAsync(ipAddress);

        [RelayCommand]
        public async Task ConnectSMBButtonAsync(string ipAddress) => await _smb.StartSMBSessionAsync(ipAddress);

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
            if (_settings.DefaultScanMode == "Auto")
            {
                IsAutoChecked = true;
                IsManualChecked = false;
            }
            else if ( _settings.DefaultScanMode == "Manual")
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

        private async void StartScanTimer()
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (_isScanning)
            {
                ScanDuration = FormatScanDuration(sw.Elapsed);
                await Task.Delay(10);
            }
        }

        private string FormatScanDuration(TimeSpan elapsedTime) =>
            $"{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}";

        private async void AddScanResults(IPScannerData data)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
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
}