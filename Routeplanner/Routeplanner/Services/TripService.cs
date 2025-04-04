using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routeplanner.Services
{
    public class TripService : ITripService
    {
        private readonly APICallService _apiCallService;

        public TripService(APICallService apiCallService)
        {
            _apiCallService = apiCallService;
        }

        public async Task<string> FetchTripsAsync(APIParameters parameters)
        {
            return await _apiCallService.GetTripsAsync(parameters);
        }

        public Task<List<Trip>> GetTrips()
        {
            throw new NotImplementedException();
        }
    }
}
