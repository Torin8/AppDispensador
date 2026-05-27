using System;
using System.Collections.Generic;
using System.Linq;
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

        // Se especifica la ruta completa explícitamente para evitar la ambigüedad
        private System.Timers.Timer _timer;
        private int _lastTriggeredMinute = -1;

        public SchedulerService(IMqttService mqttService, INotificationService notificationService)
        {
            _mqttService = mqttService;
            _notificationService = notificationService;

            // Instancia con la ruta completa
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

            // Evitamos ejecutar múltiples veces en el mismo minuto
            if (now.Minute == _lastTriggeredMinute) return;
            _lastTriggeredMinute = now.Minute;

            // Creamos una copia de la lista para evitar errores si la colección cambia
            var schedulesToProcess = _activeSchedules.ToList();

            foreach (var schedule in schedulesToProcess)
            {
                // Compara la hora actual con la hora del horario guardado
                if (schedule.Time.Hours == now.Hour && schedule.Time.Minutes == now.Minute)
                {
                    // Verifica si el día de la semana coincide
                    if (IsDayActive(schedule.ActiveDays, now.DayOfWeek))
                    {
                        // 1. Envía la orden al ESP32 vía MQTT
                        if (_mqttService.IsConnected)
                        {
                            await _mqttService.PublishAsync(AdafruitSettings.FeedTopic, "1");
                        }

                        // 2. Lanza la notificación local
                        _notificationService.ShowNotification("Dispensador Automático", $"Horario de las {schedule.DisplayTime} ejecutado. Se sirvió la comida.", schedule.Id);
                    }
                }
            }
        }

        // Traductor de días de la semana
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