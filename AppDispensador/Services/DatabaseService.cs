using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using AppDispensador.Models;

namespace AppDispensador.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        // Define la ruta de la base de datos en el almacenamiento local de la app
        private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "DogFoodDispenser.db3");

        public DatabaseService()
        {

        }

        // Inicializa la conexión y crea las tablas si no existen
        private async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);

            await _database.CreateTableAsync<Schedule>();
            await _database.CreateTableAsync<FeedEvent>();
        }

        // --- Métodos para la gestión de Horarios (Schedule) ---

        public async Task<List<Schedule>> GetSchedulesAsync()
        {
            await InitAsync();
            return await _database.Table<Schedule>().ToListAsync();
        }

        public async Task<Schedule> GetScheduleAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Schedule>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveScheduleAsync(Schedule schedule)
        {
            await InitAsync();
            if (schedule.Id != 0)
            {
                // Actualiza un horario existente
                return await _database.UpdateAsync(schedule);
            }
            else
            {
                // Inserta un nuevo horario
                return await _database.InsertAsync(schedule);
            }
        }

        public async Task<int> DeleteScheduleAsync(Schedule schedule)
        {
            await InitAsync();
            return await _database.DeleteAsync(schedule);
        }

        // --- Métodos para la gestión del Historial (FeedEvent) ---

        public async Task<List<FeedEvent>> GetFeedEventsAsync()
        {
            await InitAsync();
            // Ordena los eventos del más reciente al más antiguo
            return await _database.Table<FeedEvent>().OrderByDescending(e => e.Timestamp).ToListAsync();
        }

        public async Task<int> SaveFeedEventAsync(FeedEvent feedEvent)
        {
            await InitAsync();
            return await _database.InsertAsync(feedEvent);
        }
    }
}
