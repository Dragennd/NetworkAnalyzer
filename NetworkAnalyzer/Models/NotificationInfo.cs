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
    public NotificationType Type { get; private set; }
    private readonly MainController _mainController = App.AppHost.Services.GetRequiredService<MainController>();

    public NotificationInfo(string title, string body, NotificationType type)
    {
        Title = title;
        Body = body;
        Type = type;
        GUID = Guid.NewGuid().ToString();
        Timestamp = DateTime.Now.ToString("t");
        TitleIconColor = SetTitleIconColor();
        Icon = SetIcon();
    }

    public void ClearNotification()
    {
        _mainController.SendRemoveNotificationRequest(GUID);
    }

    private IBrush SetTitleIconColor()
    {
        return Type switch
        {
            NotificationType.Warning => Brushes.Yellow, // Warning
            NotificationType.Error => Brushes.Red, // Error
            NotificationType.Info => Brushes.DeepSkyBlue, // Information
            _ => Brushes.White // Default
        };
    }

    private MaterialIconKind SetIcon()
    {
        return Type switch
        {
            NotificationType.Warning => MaterialIconKind.AlertOutline, // Warning
            NotificationType.Error => MaterialIconKind.AlertCircleOutline, // Error
            NotificationType.Info => MaterialIconKind.InformationOutline, // Information
            _ => MaterialIconKind.About // Default
        };
    }
}

internal enum NotificationType
{
    Info,
    Warning,
    Error
}