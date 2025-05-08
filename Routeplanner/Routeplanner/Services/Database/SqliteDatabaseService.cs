using SQLite;
using Routeplanner.Model;

namespace Routeplanner.Services.Database
{
    public class SQLiteDatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public SQLiteAsyncConnection Database => _database;

        public SQLiteDatabaseService()
        {
            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        }

        public async Task InitAsync()
        {
            await _database.CreateTableAsync<Station>();
            await _database.CreateTableAsync<Route>();
            await _database.CreateTableAsync<Trip>();
            await _database.CreateTableAsync<Departure>();
            await _database.CreateTableAsync<DepartureStop>();
        }
    }
}