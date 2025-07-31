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

        public static readonly DependencyProperty StartDateTimeProperty =
            DependencyProperty.Register(nameof(StartDateTime), typeof(string), typeof(Filter));

        public static readonly DependencyProperty EndDateTimeProperty =
            DependencyProperty.Register(nameof(EndDateTime), typeof(string), typeof(Filter));

        public static readonly DependencyProperty LatencySelectionProperty =
            DependencyProperty.Register(nameof(LatencySelection), typeof(string), typeof(Filter));

        public static readonly DependencyProperty CloseFilterWindowCommandProperty =
            DependencyProperty.Register(nameof(CloseFilterWindowCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty ClearDateTimeFilterCommandProperty =
            DependencyProperty.Register(nameof(ClearDateTimeFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty SetDateTimeFilterCommandProperty =
            DependencyProperty.Register(nameof(SetDateTimeFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty ClearLatencySelectionCommandProperty =
            DependencyProperty.Register(nameof(ClearLatencySelectionCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty SetLatencySelectionFilterCommandProperty =
            DependencyProperty.Register(nameof(SetLatencySelectionFilterCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty ClearPacketLossSelectionCommandProperty =
            DependencyProperty.Register(nameof(ClearPacketLossSelectionCommand), typeof(ICommand), typeof(Filter));

        public static readonly DependencyProperty SetPacketLossSelectionCommandProperty =
            DependencyProperty.Register(nameof(SetPacketLossSelectionCommand), typeof(ICommand), typeof(Filter));

        public string StartDateTime
        {
            get => (string)GetValue(StartDateTimeProperty);
            set => SetValue(StartDateTimeProperty, value);
        }

        public string EndDateTime
        {
            get => (string)GetValue(EndDateTimeProperty);
            set => SetValue(EndDateTimeProperty, value);
        }

        public string LatencySelection
        {
            get => (string)GetValue(LatencySelectionProperty);
            set => SetValue(LatencySelectionProperty, value);
        }

        public ICommand CloseFilterWindowCommand
        {
            get => (ICommand)GetValue(CloseFilterWindowCommandProperty);
            set => SetValue(CloseFilterWindowCommandProperty, value);
        }

        public ICommand ClearDateTimeFilterCommand
        {
            get => (ICommand)GetValue(ClearDateTimeFilterCommandProperty);
            set => SetValue(ClearDateTimeFilterCommandProperty, value);
        }

        public ICommand SetDateTimeFilterCommand
        {
            get => (ICommand)GetValue(SetDateTimeFilterCommandProperty);
            set => SetValue(SetDateTimeFilterCommandProperty, value);
        }

        public ICommand ClearLatencySelectionCommand
        {
            get => (ICommand)GetValue(ClearLatencySelectionCommandProperty);
            set => SetValue(ClearLatencySelectionCommandProperty, value);
        }

        public ICommand SetLatencySelectionFilterCommand
        {
            get => (ICommand)GetValue(SetLatencySelectionFilterCommandProperty);
            set => SetValue(SetLatencySelectionFilterCommandProperty, value);
        }

        public ICommand ClearPacketLossSelectionCommand
        {
            get => (ICommand)GetValue(ClearPacketLossSelectionCommandProperty);
            set => SetValue(ClearPacketLossSelectionCommandProperty, value);
        }

        public ICommand SetPacketLossSelectionCommand
        {
            get => (ICommand)GetValue(SetPacketLossSelectionCommandProperty);
            set => SetValue(SetPacketLossSelectionCommandProperty, value);
        }
    }
}
