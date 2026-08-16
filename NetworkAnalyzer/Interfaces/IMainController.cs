using NetworkAnalyzer.EventControllers;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer.Interfaces;

internal interface IMainController
{
    event NotificationsEventHandler AddNotifications;

    void SendAddNotificationRequest(NotificationInfo notification);
}