using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Data
{
    public class TripRepository : ITripRepository
    {
        private readonly string tripstodayQuery;
        private readonly string stoptimesQuery;
        private readonly SQLiteAsyncConnection connection;

        public TripRepository(ISQLiteFactory factory)
        {
            this.connection = factory.GetConnection("gtfs");

            this.tripstodayQuery = File.ReadAllText("queries/trips_today.sql");
            this.stoptimesQuery = File.ReadAllText("queries/stoptimes.sql");
        }

        public Task<List<Trip>> GetAllTripsForToday()
        {
            return this.connection.QueryAsync<Trip>(tripstodayQuery);
        }

        public Task<List<StopTime>> GetTrip(string tripId)
        {
            return this.connection.QueryAsync<StopTime>(stoptimesQuery, tripId);
        }
    }
}
