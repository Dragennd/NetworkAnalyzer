using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.Services;

namespace NetworkAnalyzer.ViewModels;

internal partial class HomeViewModel : ObservableValidator
{
    private readonly HomeService _homeService = App.AppHost.Services.GetRequiredService<HomeService>();

    public HomeViewModel()
    {
        
    }
}