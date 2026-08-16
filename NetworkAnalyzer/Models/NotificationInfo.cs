using System;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using NetworkAnalyzer.EventControllers;

namespace NetworkAnalyzer.Models;

internal class NotificationInfo
{
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string GUID { get; private set; }
    public string Timestamp { get; private set; }
    public MaterialIconKind Icon  { get; private set; }
    public IBrush TitleIconColor  { get; private set; }
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();

    public NotificationInfo(string title, string body, MaterialIconKind icon)
    {
        Title = title;
        Body = body;
        Icon = icon;
        GUID = Guid.NewGuid().ToString();
        Timestamp = DateTime.Now.ToString("t");
        TitleIconColor = SetTitleIconColor();
    }

    public void ClearNotification()
    {
        _mainController.SendRemoveNotificationRequest(GUID);
    }

    private IBrush SetTitleIconColor()
    {
        return Icon switch
        {
            MaterialIconKind.AlertOutline => Brushes.Yellow, // Warning
            MaterialIconKind.AlertCircleOutline => Brushes.Red, // Error
            MaterialIconKind.InformationOutline => Brushes.DeepSkyBlue, // Information
            _ => Brushes.White // Default
        };
    }
}