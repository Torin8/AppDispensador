using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDispensador.Services
{
    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string message, int notificationId = 0)
        {
            // TODO: Implementar notificación local
            Console.WriteLine($"Notificación [{title}]: {message}");
        }

        public void ShowCriticalAlert(string title, string message, int notificationId = 1)
        {
            // TODO: Implementar notificación de alta prioridad
            Console.WriteLine($"ALERTA CRÍTICA [{title}]: {message}");
        }

        public void CancelNotification(int notificationId)
        {
            // TODO: Implementar cancelación de notificación
            Console.WriteLine($"Notificación {notificationId} cancelada.");
        }
    }
}
