using Avalonia.Controls;
using NetworkAnalyzer_UI_Test.Views;

namespace NetworkAnalyzer_UI_Test;

public partial class MainWindow : Window
{
    private readonly HomeView _home = new();
    private readonly LatencyMonitorView _latencyMonitorView = new();
    private readonly IPScannerView _ipScannerView = new();
    private readonly ReportsView _reportsView = new();
    private readonly SettingsView _settingsView = new();
    
    public MainWindow()
    {
        InitializeComponent();
        MainContentControl.Content = _settingsView;
    }
}