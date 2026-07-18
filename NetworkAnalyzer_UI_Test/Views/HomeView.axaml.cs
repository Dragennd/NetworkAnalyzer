using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer_UI_Test.ViewModels;

namespace NetworkAnalyzer_UI_Test.Views;

public partial class HomeView : UserControl
{
    private readonly HomeViewModel _vm = App.AppHost.Services.GetRequiredService<HomeViewModel>();
    
    public HomeView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}