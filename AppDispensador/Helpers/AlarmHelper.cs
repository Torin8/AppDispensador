using System;

namespace AppDispensador.Helpers;

public static class AlarmHelper
{
    public static string GetTimeRemainingMessage(TimeSpan alarmTime, string activeDays)
    {
        var now = DateTime.Now;
        var nextAlarm = DateTime.Today.Add(alarmTime);

        // Si la hora de la alarma ya pasó en el día actual, asumimos que será para mañana
        if (now.TimeOfDay >= alarmTime)
        {
            nextAlarm = nextAlarm.AddDays(1);
        }

        // Buscar el próximo día activo permitido en la configuración del usuario
        int daysChecked = 0;
        while (!IsDayActive(activeDays, nextAlarm.DayOfWeek) && daysChecked < 8)
        {
            nextAlarm = nextAlarm.AddDays(1);
            daysChecked++;
        }

        var diff = nextAlarm - now;

        int days = diff.Days;
        int hours = diff.Hours;
        int minutes = diff.Minutes;

        if (days == 0 && hours == 0 && minutes == 0) return "La alarma sonará en menos de un minuto.";

        string dStr = days > 0 ? $"{days} día{(days > 1 ? "s" : "")} " : "";
        string hStr = hours > 0 ? $"{hours} hora{(hours > 1 ? "s" : "")}" : "";
        string mStr = minutes > 0 ? $"{minutes} minuto{(minutes > 1 ? "s" : "")}" : "";

        string join = (hours > 0 || days > 0) && minutes > 0 ? " y " : "";

        return $"La alarma sonará en {dStr}{hStr}{join}{mStr}".TrimEnd();
    }

    private static bool IsDayActive(string activeDays, DayOfWeek currentDay)
    {
        if (string.IsNullOrEmpty(activeDays) || activeDays == "Todos los días") return true;

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
