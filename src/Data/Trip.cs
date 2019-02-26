using SQLite;

namespace Data
{
    public class Trip
    {
        [Column("trip_id")]
        public string TripId { get; set; }

        [Column("shape_id")]
        public string ShapeId { get; set; }

        [Column("departure")]
        public string DepartureTime { get; set; }

        public override string ToString()
        {
            return $"{this.DepartureTime} | {this.ShapeId}";
        }
    }
}
