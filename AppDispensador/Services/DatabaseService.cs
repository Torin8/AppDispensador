using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppDispensador.Models;

namespace AppDispensador.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;
    private readonly string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "DogFoodDispenser.db3");

    public DatabaseService()
    {
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath);

        // Se crean las tablas completamente limpias de registros
        await _database.CreateTableAsync<Schedule>();
        await _database.CreateTableAsync<FeedEvent>();
    }

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
            return await _database.UpdateAsync(schedule);
        }
        else
        {
            return await _database.InsertAsync(schedule);
        }
    }

    public async Task<int> DeleteScheduleAsync(Schedule schedule)
    {
        await InitAsync();
        return await _database.DeleteAsync(schedule);
    }

    public async Task<List<FeedEvent>> GetFeedEventsAsync()
    {
        await InitAsync();
        return await _database.Table<FeedEvent>().OrderByDescending(e => e.Timestamp).ToListAsync();
    }

    public async Task<int> SaveFeedEventAsync(FeedEvent feedEvent)
    {
        await InitAsync();
        return await _database.InsertAsync(feedEvent);
    }
}