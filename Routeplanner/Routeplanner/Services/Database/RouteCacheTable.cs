using Routeplanner.Model;

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
            // keep 5 last routes
            List<Route> routeList = await _dbService.Database.Table<Route>().ToListAsync();

            if (routeList.Count > 5)
            {
                var lastFiveRoutes = routeList.Skip(routeList.Count - 5).ToList();

                foreach (var oneRoute in lastFiveRoutes)
                {
                    await _dbService.Database.DeleteAsync(route);
                }
            }

            await _dbService.InitAsync();
            await _dbService.Database.InsertAsync(route);
        }

        public async Task<List<Route>> GetLast5Routes()
        {
            await _dbService.InitAsync();
            var allRoutes = await _dbService.Database.Table<Route>()
            .OrderByDescending(r => r.Id)
            .ToListAsync();

            var distinctRoutes = allRoutes
                .GroupBy(r => new { r.FromStation, r.ToStation }) 
                .Select(g => g.First())                         
                .Take(5)
                .ToList();

            return distinctRoutes;
        }
    }
}
