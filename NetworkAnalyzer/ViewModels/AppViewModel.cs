using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Functions;

namespace NetworkAnalyzer.ViewModels;

public partial class AppViewModel : ObservableValidator
{
    private readonly MainWindow _mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
    
    [RelayCommand]
    private void CloseWindow()
    {
        if (GlobalSettings.DefaultAppCloseBehavior == "Close")
        {
            _mainWindow.Close();
        }
        else if (GlobalSettings.DefaultAppCloseBehavior == "Minimize")
        {
            _mainWindow.Hide();
        }
    }

    [RelayCommand]
    private void ToggleWindow()
    {
        if (_mainWindow.IsVisible)
        {
            _mainWindow.Hide();
        }
        else
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }
}