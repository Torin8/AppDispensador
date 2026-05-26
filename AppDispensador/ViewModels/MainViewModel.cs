using AppDispensador.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispensador.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AppDispensador.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMqttService _mqttService;

    [ObservableProperty]
    private ObservableCollection<Schedule> _schedules;

    [ObservableProperty]
    private bool _isConnected;

    public MainViewModel(IMqttService mqttService)
    {
        _mqttService = mqttService;
        Schedules = new ObservableCollection<Schedule>();

        _mqttService.OnConnectionStatusChanged += (s, connected) =>
        {
            IsConnected = connected;
        };

        LoadSchedules();
    }

    private void LoadSchedules()
    {
        // Datos de prueba para la interfaz
        Schedules.Add(new Schedule
        {
            Time = new TimeSpan(5, 0, 0),
            ActiveDays = "Lun-vie",
            IsActive = false,
            RationGrams = 100
        });

        Schedules.Add(new Schedule
        {
            Time = new TimeSpan(6, 0, 0),
            ActiveDays = "Todos los días",
            IsActive = true,
            RationGrams = 150
        });
    }

    [RelayCommand]
    private async Task DispenseNowAsync()
    {
        if (IsConnected)
        {
            await _mqttService.PublishAsync("tu_usuario/feeds/dispensador", "1");
        }
    }
}