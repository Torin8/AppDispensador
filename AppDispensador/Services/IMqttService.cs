using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppDispensador.Services
{
    public interface IMqttService
    {
        bool IsConnected { get; }
        event EventHandler<bool> OnConnectionStatusChanged;

        // Evento que se disparará al recibir una respuesta del ESP32
        event Action<string, string> OnMessageReceived;

        Task ConnectAsync(string username, string aioKey);
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload);

        // Método para suscribir la App a un Feed específico
        Task SubscribeAsync(string topic);
    }
}
