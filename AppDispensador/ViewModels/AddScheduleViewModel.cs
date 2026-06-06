using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using AppDispensador.Models;
using AppDispensador.Services;
using AppDispensador.Helpers;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace AppDispensador.ViewModels;

[QueryProperty(nameof(ScheduleId), "id")]
public partial class AddScheduleViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly ISchedulerService _schedulerService;
    private Schedule _currentSchedule;

    [ObservableProperty]
    private int _scheduleId;

    [ObservableProperty]
    private TimeSpan _selectedTime;

    [ObservableProperty]
    private int _rationGrams = 100;

    [ObservableProperty] private bool _isMonday;
    [ObservableProperty] private bool _isTuesday;
    [ObservableProperty] private bool _isWednesday;
    [ObservableProperty] private bool _isThursday;
    [ObservableProperty] private bool _isFriday;
    [ObservableProperty] private bool _isSaturday;
    [ObservableProperty] private bool _isSunday;

    [ObservableProperty]
    private bool _isEditing;

    public AddScheduleViewModel(DatabaseService databaseService, ISchedulerService schedulerService)
    {
        _databaseService = databaseService;
        _schedulerService = schedulerService;

        SelectedTime = DateTime.Now.TimeOfDay;
    }

    partial void OnScheduleIdChanged(int value)
    {
        if (value > 0)
        {
            IsEditing = true;
            LoadScheduleAsync(value);
        }
    }

    private async void LoadScheduleAsync(int id)
    {
        _currentSchedule = await _databaseService.GetScheduleAsync(id);
        if (_currentSchedule != null)
        {
            SelectedTime = _currentSchedule.Time;
            RationGrams = _currentSchedule.RationGrams;
            ParseActiveDays(_currentSchedule.ActiveDays);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_currentSchedule == null)
        {
            _currentSchedule = new Schedule();
        }

        _currentSchedule.Time = SelectedTime;
        _currentSchedule.RationGrams = RationGrams > 0 ? RationGrams : 100;
        _currentSchedule.ActiveDays = FormatActiveDays();
        _currentSchedule.IsActive = true;

        await _databaseService.SaveScheduleAsync(_currentSchedule);
        _schedulerService.ScheduleAlarm(_currentSchedule);

        // Muestra el Toast de "tiempo restante" al confirmar el guardado
        string message = AlarmHelper.GetTimeRemainingMessage(_currentSchedule.Time, _currentSchedule.ActiveDays);
        await Toast.Make(message, ToastDuration.Long).Show();

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_currentSchedule != null && _currentSchedule.Id > 0)
        {
            await _databaseService.DeleteScheduleAsync(_currentSchedule);
            _schedulerService.CancelAlarm(_currentSchedule.Id);
        }

        await Shell.Current.GoToAsync("..");
    }

    private string FormatActiveDays()
    {
        var days = new List<string>();
        if (IsMonday) days.Add("Lun");
        if (IsTuesday) days.Add("Mar");
        if (IsWednesday) days.Add("Mié");
        if (IsThursday) days.Add("Jue");
        if (IsFriday) days.Add("Vie");
        if (IsSaturday) days.Add("Sáb");
        if (IsSunday) days.Add("Dom");

        if (days.Count == 7) return "Todos los días";
        if (days.Count == 5 && !IsSaturday && !IsSunday) return "Lun-vie";
        if (days.Count == 2 && IsSaturday && IsSunday) return "Dom sáb";

        return string.Join(" ", days);
    }

    private void ParseActiveDays(string daysString)
    {
        if (string.IsNullOrEmpty(daysString)) return;

        IsMonday = daysString.Contains("Lun") || daysString == "Todos los días" || daysString == "Lun-vie";
        IsTuesday = daysString.Contains("Mar") || daysString == "Todos los días" || daysString == "Lun-vie";
        IsWednesday = daysString.Contains("Mié") || daysString == "Todos los días" || daysString == "Lun-vie";
        IsThursday = daysString.Contains("Jue") || daysString == "Todos los días" || daysString == "Lun-vie";
        IsFriday = daysString.Contains("Vie") || daysString == "Todos los días" || daysString == "Lun-vie";
        IsSaturday = daysString.Contains("Sáb") || daysString == "Todos los días" || daysString == "Dom sáb";
        IsSunday = daysString.Contains("Dom") || daysString == "Todos los días" || daysString == "Dom sáb";
    }
}