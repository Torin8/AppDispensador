using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispensador.Models;
using AppDispensador.Services;

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

    // Método encargado de leer los datos reales guardados en SQLite
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
            await _mqttService.PublishAsync("tu_usuario/feeds/dispensador", "1");
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