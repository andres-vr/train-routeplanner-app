using Routeplanner.Model;

using System.Text.Json;

namespace Routeplanner.Services.Planner
{
    public interface ITripService
    {
        Task<List<Trip>> GetTrips();

        Task<string> FetchTripsAsync(APIParameters parameters);

        Task<string> FetchMapDataAsync(string codes);

        List<Trip> ExtractTripsFromApiResponse(JsonDocument responseData);

        void ProcessAllStopsForTrip(JsonElement tripData, Trip trip);
    }
}
