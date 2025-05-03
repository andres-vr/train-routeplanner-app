using Routeplanner.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.Services.Database
{
    public class LocalDb
    {
        private readonly string _usbDbPath = "E:\\data\\app.db";
        private readonly string _localDbPath;
        private readonly SQLiteAsyncConnection _database;

        public SQLiteAsyncConnection Database => _database;

        public LocalDb()
        {
            // Initialize paths
            _localDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MyApp",
                "local.db");

            // Create the database connection
            _database = new SQLiteAsyncConnection(_localDbPath, Constants.Flags);
        }

        public async Task InitAsync()
        {
            // Try to copy from USB if it exists
            if (File.Exists(_usbDbPath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_localDbPath));
                    File.Copy(_usbDbPath, _localDbPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying database: {ex.Message}");
                    // Continue execution even if copy fails
                }
            }

            // Create tables if they don't exist
            await _database.CreateTableAsync<Station>();
            await _database.CreateTableAsync<Route>();
            await _database.CreateTableAsync<Trip>();
            await _database.CreateTableAsync<Departure>();
            await _database.CreateTableAsync<DepartureStop>();
            await _database.CreateTableAsync<DateTimeEntry>();
        }

        public async Task CloseAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
            }
        }
    }
}
