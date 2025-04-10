using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Routeplanner.Services.Database
{
    public class RouteCacheTable
    {
        private readonly SQLiteDatabaseService _dbService;

        public RouteCacheTable(SQLiteDatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task SaveRouteToCacheAsync(Route route) 
        {
            await _dbService.Database.InsertAsync(route);
        }

        public async Task<List<Route>> GetAllRoutes()
        {
            return await _dbService.Database.Table<Route>().ToListAsync();
        }
    }
}
