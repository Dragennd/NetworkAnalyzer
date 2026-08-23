using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer.Views;

public partial class LatencyMonitorHistoryView : UserControl
{
    private readonly LatencyMonitorHistoryViewModel _vm = App.AppHost.Services.GetRequiredService<LatencyMonitorHistoryViewModel>();
    
    public LatencyMonitorHistoryView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}