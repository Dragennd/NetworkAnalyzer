using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NetworkAnalyzer.Apps.Utilities
{
    public partial class DateTimePicker : UserControl
    {
        private DateTime SelectedDate { get; set; }
        private string DateString { get; set; }
        private string Hours { get; set; }
        private string Minutes { get; set; }
        private string Seconds { get; set; }

        public DateTimePicker()
        {
            InitializeComponent();
            UpdateVisibility(IsDateTimePickerVisible);
            SetDefaultDateTimeSelection();
        }

        private static readonly DependencyProperty FinalDateTimeSelectionProperty =
            DependencyProperty.Register(nameof(FinalDateTimeSelection), typeof(string), typeof(DateTimePicker), new PropertyMetadata(default(string)));

        private static readonly DependencyProperty IsDateTimePickerVisibleProperty =
            DependencyProperty.Register(nameof(IsDateTimePickerVisible), typeof(bool), typeof(DateTimePicker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDateTimePickerVisibleChanged));

        public string FinalDateTimeSelection
        {
            get => (string)GetValue(FinalDateTimeSelectionProperty);
            set => SetValue(FinalDateTimeSelectionProperty, value);
        }

        public bool IsDateTimePickerVisible
        {
            get => (bool)GetValue(IsDateTimePickerVisibleProperty);
            set => SetValue(IsDateTimePickerVisibleProperty, value);
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Keyboard.ClearFocus();
            Mouse.Capture(null);
        }

        private void SaveDateTimeSelection(object sender, RoutedEventArgs e)
        {
            if (Calendar.SelectedDate != null)
            {
                SelectedDate = (DateTime)Calendar.SelectedDate;
                DateString = SelectedDate.ToString("MM/dd/yyyy");

                Hours = AdjustNumberFormat(TxtHours.Text);
                Minutes = AdjustNumberFormat(TxtMinutes.Text);
                Seconds = AdjustNumberFormat(TxtSeconds.Text);

                FinalDateTimeSelection = $"{DateString} {Hours}:{Minutes}:{Seconds}";

                UpdateVisibility(false);
            }
        }

        private string AdjustNumberFormat(string num)
        {
            const string SingleDigitPattern = @"^\d$";

            if (Regex.IsMatch(num, SingleDigitPattern))
            {
                return $"0{num}";
            }
            else
            {
                return num;
            }
        }

        private static void OnIsDateTimePickerVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateTimePicker control)
            {
                control.UpdateVisibility((bool)e.NewValue);
            }
        }

        private void UpdateVisibility(bool isVisible)
        {
            this.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            IsDateTimePickerVisible = isVisible;
        }

        private void SetDefaultDateTimeSelection()
        {
            Calendar.SelectedDate = DateTime.Now;
            TxtHours.Text = DateTime.Now.ToString("HH");
            TxtMinutes.Text = DateTime.Now.ToString("mm");
            TxtSeconds.Text = DateTime.Now.ToString("ss");
        }
    }
}
