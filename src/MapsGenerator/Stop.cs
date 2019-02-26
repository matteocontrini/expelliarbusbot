namespace MapsGenerator
{
    class Stop
    {
        public Stop(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
