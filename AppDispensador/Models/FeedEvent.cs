using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AppDispensador.Models
{
    public class FeedEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Fecha y hora exacta en la que se dispensó la comida
        public DateTime Timestamp { get; set; }

        // Cantidad de alimento dispensado en gramos
        public int AmountGrams { get; set; }

        // Origen del evento (ej. "Manual", "Programado")
        public string EventType { get; set; }

        // Resultado de la acción (ej. "Exitoso", "Fallido", "Plato lleno")
        public string Status { get; set; }
    }
}
