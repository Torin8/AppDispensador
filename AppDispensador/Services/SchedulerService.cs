using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AppDispensador.Models;
using AppDispensador.Config;

namespace AppDispensador.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IMqttService _mqttService;
        private readonly INotificationService _notificationService;
        private List<Schedule> _activeSchedules = new List<Schedule>();

        private System.Timers.Timer _timer;
        private int _lastTriggeredMinute = -1;

        // Bandera para rastrear si el ESP32 responde a las alarmas automáticas
        private bool _esperandoRespuestaAutomatica;

        public SchedulerService(IMqttService mqttService, INotificationService notificationService)
        {
            _mqttService = mqttService;
            _notificationService = notificationService;

            // Escuchar también aquí las respuestas para cancelar el timeout automático
            _mqttService.OnMessageReceived += (topic, payload) =>
            {
                if (topic.EndsWith("dispensador-estado", StringComparison.OrdinalIgnoreCase))
                {
                    _esperandoRespuestaAutomatica = false;
                }
            };

            _timer = new System.Timers.Timer(30000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public void ScheduleAlarm(Schedule schedule)
        {
            var existing = _activeSchedules.FirstOrDefault(s => s.Id == schedule.Id);
            if (existing != null)
                _activeSchedules.Remove(existing);

            if (schedule.IsActive)
                _activeSchedules.Add(schedule);
        }

        public void CancelAlarm(int scheduleId)
        {
            var existing = _activeSchedules.FirstOrDefault(s => s.Id == scheduleId);
            if (existing != null)
                _activeSchedules.Remove(existing);
        }

        public void UpdateAlarms(IEnumerable<Schedule> activeSchedules)
        {
            _activeSchedules = activeSchedules.Where(s => s.IsActive).ToList();
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;

            if (now.Minute == _lastTriggeredMinute) return;
            _lastTriggeredMinute = now.Minute;

            var schedulesToProcess = _activeSchedules.ToList();

            foreach (var schedule in schedulesToProcess)
            {
                if (schedule.Time.Hours == now.Hour && schedule.Time.Minutes == now.Minute)
                {
                    if (IsDayActive(schedule.ActiveDays, now.DayOfWeek))
                    {
                        if (_mqttService.IsConnected)
                        {
                            _esperandoRespuestaAutomatica = true;

                            // Envía primero la cantidad de gramos a dispensar al feed dispensador-gramos
                            await _mqttService.PublishAsync(AdafruitSettings.GramsFeedTopic, schedule.RationGrams.ToString());

                            await _mqttService.PublishAsync(AdafruitSettings.FeedTopic, "1");

                            // Inicia un temporizador de 10 segundos
                            await Task.Delay(10000);

                            // Si después de 10 segundos la bandera sigue activa, el ESP32 está apagado o sin internet
                            if (_esperandoRespuestaAutomatica)
                            {
                                _esperandoRespuestaAutomatica = false;
                                _notificationService.ShowNotification("Error de Conexión", "Error al dispensar: no se pudo establecer conexión con el dispensador", 103);
                            }
                        }
                    }
                }
            }
        }

        private bool IsDayActive(string activeDays, DayOfWeek currentDay)
        {
            if (activeDays == "Todos los días") return true;

            bool isWeekend = currentDay == DayOfWeek.Saturday || currentDay == DayOfWeek.Sunday;
            if (activeDays == "Lun-vie" && !isWeekend) return true;
            if (activeDays == "Dom sáb" && isWeekend) return true;

            string dayStr = currentDay switch
            {
                DayOfWeek.Monday => "Lun",
                DayOfWeek.Tuesday => "Mar",
                DayOfWeek.Wednesday => "Mié",
                DayOfWeek.Thursday => "Jue",
                DayOfWeek.Friday => "Vie",
                DayOfWeek.Saturday => "Sáb",
                DayOfWeek.Sunday => "Dom",
                _ => ""
            };

            return activeDays.Contains(dayStr);
        }
    }
}