using System;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

namespace AppDispensador.Services;

public class NotificationService : AppDispensador.Services.INotificationService
{
    public void ShowNotification(string title, string message, int notificationId = 0)
    {
        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = message,
            BadgeNumber = 1
            // CORRECCIÓN: Se elimina el bloque 'Schedule' para que se dispare inmediatamente
        };

        LocalNotificationCenter.Current.Show(request);
    }

    public void ShowCriticalAlert(string title, string message, int notificationId = 1)
    {
        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = message,
            BadgeNumber = 1,
            Android = new AndroidOptions
            {
                LaunchAppWhenTapped = true,
                Priority = AndroidPriority.High
            }
        };

        LocalNotificationCenter.Current.Show(request);
    }

    public void CancelNotification(int notificationId)
    {
        LocalNotificationCenter.Current.Cancel(notificationId);
    }
}