using SQLite;

namespace MapsGenerator
{
    [Table("shapes")]
    public class ShapePoint
    {
        [Column("shape_id")]
        public string ShapeId { get; set; }
        [Column("shape_pt_lat")]
        public double Latitude { get; set; }
        [Column("shape_pt_lon")]
        public double Longitude { get; set; }
        [Column("shape_pt_sequence")]
        public int Seq { get; set; }
    }
}
