using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer.Views;

public partial class IPScannerView : UserControl
{
    private readonly IPScannerViewModel _vm = App.AppHost.Services.GetRequiredService<IPScannerViewModel>();
    
    public IPScannerView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}