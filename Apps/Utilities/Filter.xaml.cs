using NetworkAnalyzer.Apps.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkAnalyzer.Utilities
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

        public ICommand CloseFilterWindowCommand
        {
            get => (ICommand)GetValue(CloseFilterWindowCommandProperty);
            set => SetValue(CloseFilterWindowCommandProperty, value);
        }
    }
}
