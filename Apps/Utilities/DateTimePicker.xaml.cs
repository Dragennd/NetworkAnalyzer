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
        }

        public static readonly DependencyProperty FinalDateTimeSelectionProperty =
            DependencyProperty.Register(nameof(FinalDateTimeSelection), typeof(string), typeof(DateTimePicker), new PropertyMetadata());

        public string FinalDateTimeSelection
        {
            get => (string)GetValue(FinalDateTimeSelectionProperty);
            private set => SetValue(FinalDateTimeSelectionProperty, value);
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

                this.Visibility = Visibility.Collapsed;
            }
        }

        private string AdjustNumberFormat(string num)
        {
            //To-Do: Add regex to check if the number is a single digit
            //and if so, precede the number with a zero
        }
    }
}
