using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteNetExtensionsAsync.Extensions;

namespace Routeplanner.Services.Database
{
    public class SavedDeparturesTable
    {
        private readonly SQLiteDatabaseService _dbService;

        public SavedDeparturesTable(SQLiteDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task SaveDepartureAsync(Departure departure)
        {
            await _dbService.InitAsync();
            await _dbService.Database.InsertWithChildrenAsync(departure, true);
        }

        public async Task RemoveDepartureAsync(Departure departure)
        {
            await _dbService.InitAsync();
            await _dbService.Database.DeleteAsync(departure, true);
        }

        public async Task<List<Departure>> GetAllDepartures()
       {
           await _dbService.InitAsync();
           return await _dbService.Database.GetAllWithChildrenAsync<Departure>();
       }
    }
}
