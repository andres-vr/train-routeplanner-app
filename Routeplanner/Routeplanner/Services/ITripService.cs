using Routeplanner.Model;

namespace Routeplanner.Services
{
    public interface ITripService
    {
        Task<List<Trip>> GetTrips();

        Task<string> FetchTripsAsync(string fromStation, string toStation, DateTime selectedDate, TimeSpan selectedTime/*, string selectedType*/);
    }
}
