using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<string> FetchTripsAsync(string fromStation, string toStation, DateTime selectedDate, TimeSpan selectedTime/*, string selectedType*/)
        {
            return await _apiCallService.GetTripsAsync(fromStation, toStation, selectedDate, selectedTime/*, selectedType*/);
        }

        public Task<List<Trip>> GetTrips()
        {
            throw new NotImplementedException();
        }
    }
}
