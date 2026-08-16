using NetworkAnalyzer.Interfaces;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.EventControllers;

internal delegate void NotificationsUpdateEventHandler(NotificationInfo notification);

internal delegate void NotificationsRemoveEventHandler(string GUID);

internal class MainController
{
    public event NotificationsUpdateEventHandler AddNotifications;

    public event NotificationsRemoveEventHandler RemoveNotifications; 

    public void SendAddNotificationRequest(NotificationInfo notification)
    {
        AddNotifications?.Invoke(notification);
    }

    public void SendRemoveNotificationRequest(string guid)
    {
        RemoveNotifications?.Invoke(guid);
    }
}