using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AppDispensador.Services;
using AppDispensador.ViewModels;
using AppDispensador.Views;
using Microsoft.Extensions.Logging;

namespace AppDispensador
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            // ==========================================
            // 1. REGISTRO DE SERVICIOS (Singleton)
            // ==========================================
            // Se usa Singleton porque deben mantener su estado global,
            // conexiones activas o conexiones únicas a la base de datos local.

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<IMqttService, MqttService>();
            builder.Services.AddSingleton<ISchedulerService>();
            builder.Services.AddSingleton<INotificationService>();

            // ==========================================
            // 2. REGISTRO DE VIEWMODELS (Transient)
            // ==========================================
            // Se usa Transient para que se cree una nueva instancia del ViewModel 
            // cada vez que el usuario navega a la página, limpiando datos residuales.

            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<AddScheduleViewModel>();

            // ==========================================
            // 3. REGISTRO DE PÁGINAS / VIEWS (Transient)
            // ==========================================
            // Es indispensable registrar las páginas con el mismo ciclo de vida 
            // que sus ViewModels correspondientes para resolver la inyección por constructor.

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AddSchedulePage>();

            
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
