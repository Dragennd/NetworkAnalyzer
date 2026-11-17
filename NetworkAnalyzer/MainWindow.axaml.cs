using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm = App.AppHost.Services.GetRequiredService<MainWindowViewModel>();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }
    
    public async void CopyCodeToClipboard(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await GetTopLevel(this)!.Clipboard!.SetTextAsync(vm.CodeToCopy);   
        }
    }
}