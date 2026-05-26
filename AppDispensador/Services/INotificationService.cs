using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDispensador.Services
{
    public interface INotificationService
    {
        // Muestra una notificación estándar (ej. "Comida servida con éxito" o "Nivel de comida bajo")
        void ShowNotification(string title, string message, int notificationId = 0);

        // Muestra una notificación de alta prioridad (ej. "Alerta Crítica: Contenedor vacío")
        void ShowCriticalAlert(string title, string message, int notificationId = 1);

        // Cancela una notificación activa si el usuario ya resolvió el problema
        void CancelNotification(int notificationId);
    }
}
