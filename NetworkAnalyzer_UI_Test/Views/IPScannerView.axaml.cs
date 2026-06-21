using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer_UI_Test.ViewModels;

namespace NetworkAnalyzer_UI_Test.Views;

public partial class IPScannerView : UserControl
{
    private readonly IPScannerViewModel _vm = App.AppHost.Services.GetRequiredService<IPScannerViewModel>();
    
    public IPScannerView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}