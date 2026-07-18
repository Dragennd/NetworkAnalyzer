using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer_UI_Test.ViewModels;

namespace NetworkAnalyzer_UI_Test.Views;

public partial class ReportsView : UserControl
{
    private readonly ReportsViewModel _vm = App.AppHost.Services.GetRequiredService<ReportsViewModel>();
    
    public ReportsView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}