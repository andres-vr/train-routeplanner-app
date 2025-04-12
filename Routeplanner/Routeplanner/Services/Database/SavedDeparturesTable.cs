using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            await _dbService.Database.InsertAsync(departure);
        }

        public async Task<List<Departure>> GetAllDepartures()
        {
            await _dbService.InitAsync();
            return await _dbService.Database.Table<Departure>().ToListAsync();
        }
    }
}
