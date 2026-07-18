using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer.Views;

public partial class ReportsView : UserControl
{
    private readonly ReportsViewModel _vm = App.AppHost.Services.GetRequiredService<ReportsViewModel>();
    
    public ReportsView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}