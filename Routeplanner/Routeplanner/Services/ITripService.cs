using Routeplanner.Model;

namespace Routeplanner.Services
{
    public interface ITripService
    {
        Task<List<Trip>> GetMonkeys();
    }
}
