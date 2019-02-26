using SQLite;

namespace Data
{
    public class StopTime
    {
        [Column("departure_time")]
        public string DepartureTime { get; set; }

        [Column("stop_id")]
        public string StopId { get; set; }

        [Column("stop_name")]
        public string StopName { get; set; }

        public override string ToString()
        {
            return $"{DepartureTime} @ {StopName}";
        }
    }
}
