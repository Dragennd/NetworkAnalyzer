using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetworkAnalyzer.Apps.IPScanner.Interfaces;
using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public int TotalAddressCount
        {
            get => _ipScannerService.TotalAddressCount;
            set
            {
                if (_ipScannerService.TotalAddressCount != value)
                {
                    _ipScannerService.TotalAddressCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalActiveAddresses
        {
            get => _ipScannerService.TotalActiveAddresses;
            set
            {
                if (_ipScannerService.TotalActiveAddresses != value)
                {
                    _ipScannerService.TotalActiveAddresses = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalInactiveAddresses
        {
            get => _ipScannerService.TotalInactiveAddresses;
            set
            {
                if (_ipScannerService.TotalInactiveAddresses != value)
                {
                    _ipScannerService.TotalInactiveAddresses = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public bool isStartButtonEnabled = true;

        [ObservableProperty]
        public bool isAutoChecked = true;

        [ObservableProperty]
        public bool isManualChecked = false;

        private readonly IIPScannerService _ipScannerService;

        private readonly IIPScannerController _ipScannerController;
        #endregion Properties

        public IPScannerViewModel(IIPScannerService ipScannerService, IIPScannerController ipScannerController)
        {
            AllScanResults = new();
            _ipScannerService = ipScannerService;
            _ipScannerController = ipScannerController;

            _ipScannerService.PropertyChanged += IPScannerService_PropertyChanged;
            _ipScannerController.AddScanResults += AddResults;
            
        }

        [RelayCommand]
        public async Task StartIPScanButtonAsync()
        {
            IsStartButtonEnabled = false;
            await _ipScannerService.StartScanAsync(IsAutoChecked);
            IsStartButtonEnabled = true;
        }

        #region Private Methods
        private void AddResults(IPScannerData data)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AllScanResults.Add(data);
            }));
        }

        private void IPScannerService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPScannerService.TotalInactiveAddresses))
            {
                OnPropertyChanged(nameof(TotalInactiveAddresses));
            }
            else if (e.PropertyName == nameof(IPScannerService.TotalActiveAddresses))
            {
                OnPropertyChanged(nameof(TotalActiveAddresses));
            }
            else if (e.PropertyName == nameof(IPScannerService.TotalAddressCount))
            {
                OnPropertyChanged(nameof(TotalAddressCount));
            }
            else if (e.PropertyName == nameof(IPScannerService.ScanDuration))
            {
                OnPropertyChanged(nameof(ScanDuration));
            }
            else if (e.PropertyName == nameof(IPScannerService.ScanStatus))
            {
                OnPropertyChanged(nameof(ScanStatus));
            }
            else if (e.PropertyName == nameof(IPScannerService.SubnetsToScan))
            {
                OnPropertyChanged(nameof(SubnetsToScan));
            }
        }
        #endregion Private Methods
    }
}