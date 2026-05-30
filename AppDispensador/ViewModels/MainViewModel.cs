using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispensador.Models;
using AppDispensador.Services;
using AppDispensador.Config;
using Microsoft.Maui.ApplicationModel;

namespace AppDispensador.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMqttService _mqttService;
    private readonly DatabaseService _databaseService;
    private readonly INotificationService _notificationService;
    private readonly ISchedulerService _schedulerService;

    [ObservableProperty]
    private ObservableCollection<Schedule> _schedules;

    [ObservableProperty]
    private bool _isConnected;

    // Bandera para rastrear si el ESP32 responde al comando manual
    private bool _esperandoRespuestaManual;

    public MainViewModel(IMqttService mqttService, DatabaseService databaseService, INotificationService notificationService, ISchedulerService schedulerService)
    {
        _mqttService = mqttService;
        _databaseService = databaseService;
        _notificationService = notificationService;
        _schedulerService = schedulerService;
        Schedules = new ObservableCollection<Schedule>();

        _mqttService.OnConnectionStatusChanged += async (s, connected) =>
        {
            IsConnected = connected;
            if (connected)
            {
                await _mqttService.SubscribeAsync($"{AdafruitSettings.Username}/feeds/dispensador-estado");
            }
        };

        _mqttService.OnMessageReceived += (topic, payload) =>
        {
            if (topic.EndsWith("dispensador-estado", StringComparison.OrdinalIgnoreCase))
            {
                // Se recibió respuesta, cancelamos el timeout de error
                _esperandoRespuestaManual = false;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    string cleanPayload = payload.Trim().ToLower();

                    if (cleanPayload == "s")
                    {
                        string horaActual = DateTime.Now.ToString("hh:mm tt");
                        _notificationService.ShowNotification("Dispensador", $"Se dispensó la comida a las {horaActual}", 100);
                    }
                    else if (cleanPayload == "c")
                    {
                        _notificationService.ShowNotification("Alerta de Contenedor", "Acción bloqueada: No hay suficiente comida en el contenedor principal.", 101);
                    }
                    else if (cleanPayload == "p")
                    {
                        _notificationService.ShowNotification("Alerta de Plato", "Acción bloqueada: El plato ya tiene comida.", 102);
                    }
                });
            }
        };
    }

    public async Task UpdateScheduleStatusAsync(Schedule schedule)
    {
        await _databaseService.SaveScheduleAsync(schedule);
        var dbSchedules = await _databaseService.GetSchedulesAsync();
        _schedulerService.UpdateAlarms(dbSchedules);
    }

    [RelayCommand]
    public async Task ConnectMqttAsync()
    {
        if (!_mqttService.IsConnected)
        {
            try
            {
                await _mqttService.ConnectAsync(AdafruitSettings.Username, AdafruitSettings.AioKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión MQTT: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public async Task LoadSchedulesAsync()
    {
        Schedules.Clear();
        var dbSchedules = await _databaseService.GetSchedulesAsync();
        foreach (var schedule in dbSchedules)
        {
            Schedules.Add(schedule);
        }
    }

    [RelayCommand]
    private async Task DispenseNowAsync()
    {
        if (IsConnected)
        {
            _esperandoRespuestaManual = true;
            await _mqttService.PublishAsync(AdafruitSettings.FeedTopic, "1");

            // Inicia un temporizador de 10 segundos
            await Task.Delay(10000);

            // Si después de 10 segundos la bandera sigue activa, el ESP32 no respondió
            if (_esperandoRespuestaManual)
            {
                _esperandoRespuestaManual = false;
                _notificationService.ShowNotification("Error de Conexión", "Error al dispensar: no se pudo establecer conexión con el dispensador", 103);
            }
        }
    }

    [RelayCommand]
    private async Task NavigateToAddScheduleAsync()
    {
        await Shell.Current.GoToAsync("AddSchedulePage");
    }

    [RelayCommand]
    private async Task EditScheduleAsync(Schedule schedule)
    {
        if (schedule == null) return;
        await Shell.Current.GoToAsync($"AddSchedulePage?id={schedule.Id}");
    }
}