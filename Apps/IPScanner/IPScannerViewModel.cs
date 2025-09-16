using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using System.Collections.Concurrent;

namespace NetworkAnalyzer.Apps.IPScanner
{
    internal partial class IPScannerViewModel : ObservableValidator
    {
        #region Control Properties
        public ConcurrentBag<IPScannerData> AllScanResults
        {
            get => _ipScannerService.AllScanResults;
            set
            {
                if (_ipScannerService.AllScanResults != value)
                {
                    _ipScannerService.AllScanResults = value;
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

        public string ScanStatus
        {
            get => _ipScannerService.ScanStatus;
            set
            {
                if (_ipScannerService.ScanStatus != value)
                {
                    _ipScannerService.ScanStatus = value;
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
        public int totalAddressCount;

        [ObservableProperty]
        public int totalActiveAddresses;

        [ObservableProperty]
        public int totalInactiveAddresses;

        [ObservableProperty]
        public bool isStartButtonEnabled;

        [ObservableProperty]
        public bool autoChecked;

        [ObservableProperty]
        public bool manualChecked;

        private readonly IIPScannerService _ipScannerService;
        #endregion Control Properties

        public IPScannerViewModel(IIPScannerService ipScannerService)
        {
            _ipScannerService = ipScannerService;
        }

        [RelayCommand]
        public async Task StartIPScanButtonAsync()
        {
            await _ipScannerService.StartScan();
        }

        #region Private Methods
        
        #endregion Private Methods
    }
}