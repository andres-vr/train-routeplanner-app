using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;

namespace Routeplanner.Services.Database
{
    public class SavedTripsTable
    {
        private readonly SQLiteDatabaseService _dbService;

        public SavedTripsTable(SQLiteDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task SaveTripAsync(Trip trip)
        {
            await _dbService.InitAsync();
            await _dbService.Database.InsertWithChildrenAsync(trip, true);
        }

        public async Task<List<Trip>> GetAllTrips()
        {
            await _dbService.InitAsync();
            return await _dbService.Database.GetAllWithChildrenAsync<Trip>();
        }
    }
}
