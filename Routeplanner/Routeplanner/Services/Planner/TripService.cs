using Routeplanner.Model;
using Routeplanner.Services.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routeplanner.Services.Planner
{
    public class TripService : ITripService
    {
        private readonly TripsAPICallService _apiCallService;

        public TripService(TripsAPICallService apiCallService)
        {
            _apiCallService = apiCallService;
        }

        public async Task<string> FetchTripsAsync(APIParameters parameters)
        {
            return await _apiCallService.MakeCallAsync(parameters);
        }

        public Task<List<Trip>> GetTrips()
        {
            throw new NotImplementedException();
        }
    }
}
