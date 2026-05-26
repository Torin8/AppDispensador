using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using AppDispensador.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            BadgeNumber = 1,
            Schedule = { NotifyTime = DateTime.Now }
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
            Schedule = { NotifyTime = DateTime.Now },
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