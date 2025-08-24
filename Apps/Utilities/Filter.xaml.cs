using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetworkAnalyzer.Apps.Utilities
{
    public partial class Filter : UserControl
    {
        public Filter()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ActiveFiltersProperty =
            DependencyProperty.Register(nameof(ActiveFilters), typeof(ObservableCollection<FilterData>), typeof(Filter));

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

        public static readonly DependencyProperty CloseFilterWindowCommandProperty =
            DependencyProperty.Register(nameof(CloseFilterWindowCommand), typeof(ICommand), typeof(Filter));

        public ObservableCollection<FilterData> ActiveFilters
        {
            get => (ObservableCollection<FilterData>)GetValue(ActiveFiltersProperty);
            set => SetValue(ActiveFiltersProperty, value);
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
