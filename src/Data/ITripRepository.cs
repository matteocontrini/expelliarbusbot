using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data
{
    public interface ITripRepository
    {
        Task<List<Trip>> GetAllTripsForToday();

        Task<List<StopTime>> GetTrip(string tripId);
    }
}
