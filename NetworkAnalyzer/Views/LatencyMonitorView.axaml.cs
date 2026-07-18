using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer.Views;

internal partial class LatencyMonitorView : UserControl
{
    private readonly LatencyMonitorViewModel _vm = App.AppHost.Services.GetRequiredService<LatencyMonitorViewModel>();
    
    public LatencyMonitorView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}