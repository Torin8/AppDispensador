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

        private System.Timers.Timer _timer;
        private int _lastTriggeredMinute = -1;

        public SchedulerService(IMqttService mqttService, INotificationService notificationService)
        {
            _mqttService = mqttService;
            _notificationService = notificationService;

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
                            // Únicamente mandamos la orden. La notificación ahora la genera el MqttService al recibir la confirmación de estado
                            await _mqttService.PublishAsync(AdafruitSettings.FeedTopic, "1");
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