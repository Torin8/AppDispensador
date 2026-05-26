using AppDispensador.Services;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace AppDispensador.Services;

public class MqttService : IMqttService
{
    private IMqttClient _mqttClient;
    private MqttFactory _mqttFactory;

    public bool IsConnected => _mqttClient?.IsConnected ?? false;
    public event EventHandler<bool> OnConnectionStatusChanged;

    public MqttService()
    {
        _mqttFactory = new MqttFactory();
        _mqttClient = _mqttFactory.CreateMqttClient();

        _mqttClient.DisconnectedAsync += async e =>
        {
            OnConnectionStatusChanged?.Invoke(this, false);

            await Task.Delay(TimeSpan.FromSeconds(5));
            // Lógica de reconexión aquí
        };
    }

    public async Task ConnectAsync(string username, string aioKey)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId(Guid.NewGuid().ToString())
            .WithTcpServer("io.adafruit.com", 8883)
            .WithCredentials(username, aioKey)
            .WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                SslProtocol = SslProtocols.Tls12
            })
            .WithCleanSession()
            .Build();

        await _mqttClient.ConnectAsync(options);
        OnConnectionStatusChanged?.Invoke(this, true);
    }

    public async Task DisconnectAsync()
    {
        await _mqttClient.DisconnectAsync();
        OnConnectionStatusChanged?.Invoke(this, false);
    }

    public async Task PublishAsync(string topic, string payload)
    {
        if (!IsConnected) return;

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _mqttClient.PublishAsync(message);
    }
}