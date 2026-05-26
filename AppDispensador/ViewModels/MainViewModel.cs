using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispensador.Models;
using AppDispensador.Services;
using AppDispensador.Config;

namespace AppDispensador.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMqttService _mqttService;
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Schedule> _schedules;

    [ObservableProperty]
    private bool _isConnected;

    public MainViewModel(IMqttService mqttService, DatabaseService databaseService)
    {
        _mqttService = mqttService;
        _databaseService = databaseService;
        Schedules = new ObservableCollection<Schedule>();

        _mqttService.OnConnectionStatusChanged += (s, connected) =>
        {
            IsConnected = connected;
        };
    }

    [RelayCommand]
    public async Task ConnectMqttAsync()
    {
        if (!_mqttService.IsConnected)
        {
            try
            {
                // Vinculación segura desde el archivo externo protegido (.gitignore)
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
            // Publicación dinámica al FeedTopic seguro sin Hardcodeo expuesto
            await _mqttService.PublishAsync(AdafruitSettings.FeedTopic, "1");
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