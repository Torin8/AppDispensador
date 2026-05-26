using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppDispensador.Models;

namespace AppDispensador.Services
{
    public class SchedulerService : ISchedulerService
    {
        public void ScheduleAlarm(Schedule schedule)
        {
            // TODO: Implementar lógica nativa (ej. AlarmManager en Android)
            Console.WriteLine($"Alarma programada para el horario: {schedule.Time}");
        }

        public void CancelAlarm(int scheduleId)
        {
            // TODO: Implementar cancelación de alarma
            Console.WriteLine($"Alarma cancelada. ID: {scheduleId}");
        }

        public void UpdateAlarms(IEnumerable<Schedule> activeSchedules)
        {
            // TODO: Implementar recarga de alarmas activas
            Console.WriteLine($"Sincronizando {activeSchedules.Count()} alarmas activas.");
        }
    }
}
