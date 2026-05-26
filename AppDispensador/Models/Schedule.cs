using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AppDispensador.Models
{
    public class Schedule
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Hora en formato TimeSpan para facilitar operaciones
        public TimeSpan Time { get; set; }

        // Días activos (ej. "1,2,3,4,5" para Lunes a Viernes)
        public string ActiveDays { get; set; }

        public bool IsActive { get; set; }

        // Cantidad en gramos
        public int RationGrams { get; set; }

        // Formato visual para la UI (ej. "05:00")
        [Ignore]
        public string DisplayTime => $"{Time.Hours:D2}:{Time.Minutes:D2}";
    }
}
