using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NetworkAnalyzer_UI_Test.ViewModels;

namespace NetworkAnalyzer_UI_Test.Views;

internal partial class LatencyMonitorView : UserControl
{
    public LatencyMonitorView(LatencyMonitorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}