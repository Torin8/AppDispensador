using Microsoft.Maui.Controls;
using AppDispensador.ViewModels;
using AppDispensador.Models;
using AppDispensador.Helpers;
using Plugin.LocalNotification;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace AppDispensador.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private bool _isPageLoaded = false;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public MainPage()
    {
        InitializeComponent();
#if WINDOWS || ANDROID || IOS || MACCATALYST
        _viewModel = IPlatformApplication.Current?.Services.GetService<MainViewModel>();
        BindingContext = _viewModel;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isPageLoaded = false; // Detiene eventos falsos al cargar

        if (_viewModel != null)
        {
            await _viewModel.LoadSchedulesAsync();

            // SOLUCIÓN A NOTIFICACIONES: Solicitar permiso explícito al usuario del teléfono
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            await _viewModel.ConnectMqttAsync();
        }

        _isPageLoaded = true; // Habilita eventos visuales
    }

    private async void OnAlarmToggled(object sender, ToggledEventArgs e)
    {
        // Solo ejecuta la acción si el usuario lo presionó manualmente
        if (!_isPageLoaded) return;

        if (sender is Switch toggleSwitch && toggleSwitch.BindingContext is Schedule schedule)
        {
            // Guarda el cambio de estado en Base de Datos y recarga el temporizador
            await _viewModel.UpdateScheduleStatusAsync(schedule);

            // Si el switch se encendió, muestra la ventana flotante con el tiempo restante
            if (e.Value)
            {
                string message = AlarmHelper.GetTimeRemainingMessage(schedule.Time, schedule.ActiveDays);
                await Toast.Make(message, ToastDuration.Long).Show();
            }
        }
    }
}
