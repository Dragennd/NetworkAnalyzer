using CommunityToolkit.Mvvm.ComponentModel;

namespace NetworkAnalyzer.Apps.Reports
{
    internal partial class ReportsViewModel : ObservableValidator
    {
        [ObservableProperty]
        public bool isAllDataChecked = true;

        [ObservableProperty]
        public bool isDateRangeChecked = false;

        [ObservableProperty]
        public double dataSetBorderHeight = 325;
    }
}
