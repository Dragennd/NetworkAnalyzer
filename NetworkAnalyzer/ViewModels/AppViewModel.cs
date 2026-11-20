using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkAnalyzer.ViewModels;

public partial class AppViewModel : ObservableValidator
{
    [ObservableProperty]
    public partial string AppVisibilityTitle { get; set; } = "Hide Network Analyzer";
    
    private readonly MainWindow _mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
    
    [RelayCommand]
    private void CloseWindow()
    {
        _mainWindow.Close();
    }

    [RelayCommand]
    private void ToggleWindow()
    {
        if (_mainWindow.IsVisible)
        {
            AppVisibilityTitle = "Show Network Analyzer";
            _mainWindow.Hide();
        }
        else
        {
            AppVisibilityTitle = "Hide Network Analyzer";
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }
}