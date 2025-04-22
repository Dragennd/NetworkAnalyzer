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
            DependencyProperty.Register(nameof(StartDateTime), typeof(string), typeof(ManagePresets));

        public static readonly DependencyProperty EndDateTimeProperty =
            DependencyProperty.Register(nameof(EndDateTime), typeof(string), typeof(ManagePresets));

        public static readonly DependencyProperty LatencySelectionProperty =
            DependencyProperty.Register(nameof(LatencySelection), typeof(string), typeof(ManagePresets));

        public static readonly DependencyProperty CloseWindowCommandProperty =
            DependencyProperty.Register(nameof(CloseWindowCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty ClearDateTimeFilterCommandProperty =
            DependencyProperty.Register(nameof(ClearDateTimeFilterCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty SetDateTimeFilterCommandProperty =
            DependencyProperty.Register(nameof(SetDateTimeFilterCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty ClearLatencySelectionCommandProperty =
            DependencyProperty.Register(nameof(CleadLatencySelectionCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty SetLatencySelectionFilterCommandProperty =
            DependencyProperty.Register(nameof(SetLatencySelectionFilterCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty ClearPacketLossSelectionCommandProperty =
            DependencyProperty.Register(nameof(ClearPacketLossSelectionCommand), typeof(ICommand), typeof(ManagePresets));

        public static readonly DependencyProperty SetPacketLossSelectionCommandProperty =
            DependencyProperty.Register(nameof(SetPacketLossSelectionCommand), typeof(ICommand), typeof(ManagePresets));

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

        public ICommand CloseWindowCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand ClearDateTimeFilterCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand SetDateTimeFilterCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand CleadLatencySelectionCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand SetLatencySelectionFilterCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand ClearPacketLossSelectionCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }

        public ICommand SetPacketLossSelectionCommand
        {
            get => (ICommand)GetValue(CloseWindowCommandProperty);
            set => SetValue(CloseWindowCommandProperty, value);
        }
    }
}
