using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AppDispensador.Services;
using AppDispensador.ViewModels;
using AppDispensador.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace AppDispensador;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ==========================================
        // 1. REGISTRO DE SERVICIOS (Singleton)
        // ==========================================
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IMqttService, MqttService>();
        builder.Services.AddSingleton<ISchedulerService, SchedulerService>();

        // CORRECCIÓN: Se especifica el namespace completo para evitar la ambigüedad
        builder.Services.AddSingleton<AppDispensador.Services.INotificationService, NotificationService>();

        // ==========================================
        // 2. REGISTRO DE VIEWMODELS (Transient)
        // ==========================================
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AddScheduleViewModel>();

        // ==========================================
        // 3. REGISTRO DE PÁGINAS / VIEWS (Transient)
        // ==========================================
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddSchedulePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}