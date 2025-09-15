using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NetworkAnalyzer.Apps.Utilities
{
    public partial class Filter : UserControl
    {
        public Filter()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ActiveFiltersProperty =
            DependencyProperty.Register(nameof(ActiveFilters), typeof(ObservableCollection<FilterData>), typeof(Filter), new PropertyMetadata(null));

        public static readonly DependencyProperty UserDefinedTargetsProperty =
            DependencyProperty.Register(nameof(UserDefinedTargets), typeof(ObservableCollection<LatencyMonitorData>), typeof(Filter), new PropertyMetadata(null));

        public static readonly DependencyProperty TracerouteTargetsProperty =
            DependencyProperty.Register(nameof(TracerouteTargets), typeof(ObservableCollection<LatencyMonitorReportEntries>), typeof(Filter), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedUserDefinedTargetProperty =
            DependencyProperty.Register(nameof(SelectedUserDefinedTarget), typeof(LatencyMonitorData), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty SelectedTracerouteTargetProperty =
            DependencyProperty.Register(nameof(SelectedTracerouteTarget), typeof(LatencyMonitorReportEntries), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty SelectedActiveFilterProperty =
            DependencyProperty.Register(nameof(SelectedActiveFilter), typeof(FilterData), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty SelectedFilterTypeProperty =
            DependencyProperty.Register(nameof(SelectedFilterType), typeof(FilterType), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty SelectedFilterOperatorProperty =
            DependencyProperty.Register(nameof(SelectedFilterOperator), typeof(FilterOperator), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty SelectedBinaryFilterOperatorProperty =
            DependencyProperty.Register(nameof(SelectedBinaryFilterOperator), typeof(BinaryFilterOperator), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty FilterValueProperty =
            DependencyProperty.Register(nameof(FilterValue), typeof(string), typeof(Filter), new PropertyMetadata());

        public static readonly DependencyProperty ApplyFilterCommandProperty =
            DependencyProperty.Register(nameof(ApplyFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty FetchHistoryDataCommandProperty =
            DependencyProperty.Register(nameof(FetchHistoryDataCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty RemoveFilterCommandProperty =
            DependencyProperty.Register(nameof(RemoveFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty SetAddressFilterCommandProperty =
            DependencyProperty.Register(nameof(SetAddressFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty CloseFilterWindowCommandProperty =
            DependencyProperty.Register(nameof(CloseFilterWindowCommand), typeof(ICommand), typeof(Filter));

        public ObservableCollection<FilterData> ActiveFilters
        {
            get => (ObservableCollection<FilterData>)GetValue(ActiveFiltersProperty);
            set => SetValue(ActiveFiltersProperty, value);
        }

        public ObservableCollection<LatencyMonitorData> UserDefinedTargets
        {
            get => (ObservableCollection<LatencyMonitorData>)GetValue(UserDefinedTargetsProperty);
            set => SetValue(UserDefinedTargetsProperty, value);
        }

        public ObservableCollection<LatencyMonitorReportEntries> TracerouteTargets
        {
            get => (ObservableCollection<LatencyMonitorReportEntries>)GetValue(TracerouteTargetsProperty);
            set => SetValue(TracerouteTargetsProperty, value);
        }

        public LatencyMonitorData SelectedUserDefinedTarget
        {
            get => (LatencyMonitorData)GetValue(SelectedUserDefinedTargetProperty);
            set => SetValue(SelectedUserDefinedTargetProperty, value);
        }

        public LatencyMonitorReportEntries SelectedTracerouteTarget
        {
            get => (LatencyMonitorReportEntries)GetValue(SelectedTracerouteTargetProperty);
            set => SetValue(SelectedTracerouteTargetProperty, value);
        }

        public FilterData SelectedActiveFilter
        {
            get => (FilterData)GetValue(SelectedActiveFilterProperty);
            set => SetValue(SelectedActiveFilterProperty, value);
        }

        public FilterType SelectedFilterType
        {
            get => (FilterType)GetValue(SelectedFilterTypeProperty);
            set => SetValue(SelectedFilterTypeProperty, value);
        }

        public FilterOperator SelectedFilterOperator
        {
            get => (FilterOperator)GetValue(SelectedFilterOperatorProperty);
            set => SetValue(SelectedFilterOperatorProperty, value);
        }

        public BinaryFilterOperator SelectedBinaryFilterOperator
        {
            get => (BinaryFilterOperator)GetValue(SelectedBinaryFilterOperatorProperty);
            set => SetValue(SelectedBinaryFilterOperatorProperty, value);
        }

        public string FilterValue
        {
            get => (string)GetValue(FilterValueProperty);
            set => SetValue(FilterValueProperty, value);
        }

        public ICommand ApplyFilterCommand
        {
            get => (ICommand)GetValue(ApplyFilterCommandProperty);
            set => SetValue(ApplyFilterCommandProperty, value);
        }

        public ICommand FetchHistoryDataCommand
        {
            get => (ICommand)GetValue(FetchHistoryDataCommandProperty);
            set => SetValue(FetchHistoryDataCommandProperty, value);
        }

        public ICommand RemoveFilterCommand
        {
            get => (ICommand)GetValue(RemoveFilterCommandProperty);
            set => SetValue(RemoveFilterCommandProperty, value);
        }

        public ICommand SetAddressFilterCommand
        {
            get => (ICommand)GetValue(SetAddressFilterCommandProperty);
            set => SetValue(SetAddressFilterCommandProperty, value);
        }

        public ICommand CloseFilterWindowCommand
        {
            get => (ICommand)GetValue(CloseFilterWindowCommandProperty);
            set => SetValue(CloseFilterWindowCommandProperty, value);
        }

        private void ShowDateTimePickerWindow(object sender, RoutedEventArgs e)
        {
            if (FilterDateTimePicker.Visibility == Visibility.Visible)
            {
                FilterDateTimePicker.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterDateTimePicker.Visibility = Visibility.Visible;
            }
        }
    }
}
