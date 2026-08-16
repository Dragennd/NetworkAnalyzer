using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.EventControllers;

internal delegate void NotificationsEventHandler(NotificationInfo notification);

internal class MainController : IMainController
{
    public event NotificationsEventHandler AddNotifications;

    public void SendAddNotificationRequest(NotificationInfo notification)
    {
        AddNotifications?.Invoke(notification);
    }
}