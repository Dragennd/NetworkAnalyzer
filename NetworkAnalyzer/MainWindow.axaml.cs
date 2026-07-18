using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using NetworkAnalyzer.ViewModels;

namespace NetworkAnalyzer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public async void CopyCodeToClipboard(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await GetTopLevel(this)!.Clipboard!.SetTextAsync(vm.CodeToCopy);   
        }
    }
}