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
        Task ConnectAsync(string username, string aioKey);
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload);
    }
}
