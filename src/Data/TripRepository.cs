using NodaTime;
using NodaTime.Text;
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
            // Get today's date using Italy's timezone
            Instant now = SystemClock.Instance.GetCurrentInstant();
            DateTimeZone tz = DateTimeZoneProviders.Tzdb["Europe/Rome"];
            LocalDate today = now.InZone(tz).Date;
            LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd");
            string date = pattern.Format(today);

            return this.connection.QueryAsync<Trip>(tripstodayQuery, date, date, date, date);
        }

        public Task<List<StopTime>> GetTrip(string tripId)
        {
            return this.connection.QueryAsync<StopTime>(stoptimesQuery, tripId);
        }
    }
}
