using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer.Views;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _vm = App.AppHost.Services.GetRequiredService<SettingsViewModel>();
    
    public SettingsView()
    {
        InitializeComponent();
        DataContext = _vm;
    }
}