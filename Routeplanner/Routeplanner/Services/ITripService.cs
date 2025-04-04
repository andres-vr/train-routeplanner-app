using Routeplanner.Model;
using System.Text.Json;

namespace Routeplanner.Services
{
    public interface ITripService
    {
        Task<List<Trip>> GetTrips();

        Task<string> FetchTripsAsync(APIParameters parameters);
    }
}
