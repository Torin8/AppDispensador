using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppDispensador.Models;

namespace AppDispensador.Services
{
    public interface ISchedulerService
    {
        // Programa una alarma o tarea en segundo plano para un horario específico
        void ScheduleAlarm(Schedule schedule);

        // Cancela una alarma previamente programada usando su ID
        void CancelAlarm(int scheduleId);

        // Sincroniza o recarga todas las alarmas activas 
        // (útil al reiniciar el dispositivo o abrir la app)
        void UpdateAlarms(IEnumerable<Schedule> activeSchedules);
    }
}
