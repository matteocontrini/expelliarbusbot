using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SQLite;

namespace MapsGenerator
{
    /// <summary>
    /// Console application to generate maps once using MapBox.
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            string databaseFilePath = @"C:\Projects\expelliarbusbot\stuff\queries\gtfs_2021_09_10.db";

            // mapbox public token
            string token = "";

            var db = new SQLiteConnection(databaseFilePath);

            Stop[] stops = new Stop[]
            {
                new Stop(46.065955, 11.154596), // 0 manci
                new Stop(46.063862, 11.151912), // 1 centro civico
                new Stop(46.063947, 11.150560), // 2 pantè
                new Stop(46.063316, 11.150209), // 3 fac. scienze
                new Stop(46.067348, 11.150372), // 4 scientifico ovest
                new Stop(46.065746, 11.146326), // 5 valoni
            };

            // You can obtain the list of shapes with the query "all shapes linea 5.sql".
            // Then use https://github.com/google/transitfeed/wiki/ScheduleViewer to see the paths

            List<(string, Stop[])> shapes = new List<(string, Stop[])>
            {
                // passa da centro civico
                ( "D174_F0512_Ritorno_sub2", new Stop[] { stops[0], stops[1], stops[3], stops[5] } ),
                // da polo sociale
                ( "D607_T0526c_Ritorno_sub1", new Stop[] { stops[0], stops[2], stops[5] } ),
                // da oltrecastello
                ( "D609_T0530_Ritorno_sub2", new Stop[] { stops[0], stops[2], stops[5] } ),
                // da polo scientifico
                ( "D617_T0542_Ritorno_sub2", new Stop[] { stops[4], stops[5] } ),
                // passa da centro civico
                ( "D608_T0528c_Ritorno_sub1", new Stop[] { stops[0], stops[1], stops[3], stops[5] } ),
                // da polo sociale, passa da polo scientifico
                ( "D605_T0522j_Ritorno_sub1", new Stop[] { stops[0], stops[2], stops[4], stops[5] } ),
            };

            foreach ((string shapeId, Stop[] s) in shapes)
            {
                Console.WriteLine(shapeId);
                Console.WriteLine();

                var points = db.Table<ShapePoint>()
                               .Where(x => x.ShapeId == shapeId)
                               .OrderBy(x => x.ShapeId)
                               .ToList();

                string polyline = Encode(points);
                string encoded = WebUtility.UrlEncode(polyline);

                StringBuilder pins = new StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    string lon = s[i].Lon.ToString().Replace(',', '.');
                    string lat = s[i].Lat.ToString().Replace(',', '.');
                    pins.Append($",pin-l-{i+1}+E94335({lon},{lat})");
                }

                // old with fixed stops
                //string url = $"https://api.mapbox.com/styles/v1/matteocontrini/cjslp4zzs5fef1fpeowgokfff/static/path-5+f5c500({encoded}),pin-l-a+E94335(11.150372,46.067348),pin-l-b+E94335(11.154596,46.065955),pin-l-c+E94335(11.151912,46.063862),pin-l-e+E94335(11.150209,46.063316),pin-l-d+E94335(11.150560,46.063947),pin-l-f+E94335(11.146326,46.065746)/11.1507,46.0653,15.3,0,0/600x600@2x?access_token={token}";
                string url = $"https://api.mapbox.com/styles/v1/matteocontrini/cjslp4zzs5fef1fpeowgokfff/static/path-5+f5c500({encoded}){pins}/11.1507,46.0653,15.3,0,0/600x600@2x?access_token={token}";

                Console.WriteLine(url);
                Console.WriteLine();

                using (var client = new WebClient())
                {
                    // Uncomment to download and save map images
                    client.DownloadFile(url, $"{shapeId}.png");
                }
            }

            Console.WriteLine("OKI DOKI");
            Console.ReadLine();
        }

        // Thank you stranger
        // https://stackoverflow.com/a/3852420/1633924
        public static string Encode(IEnumerable<ShapePoint> points)
        {
            var str = new StringBuilder();

            var encodeDiff = (Action<int>)(diff => {
                int shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;
                int rem = shifted;
                while (rem >= 0x20)
                {
                    str.Append((char)((0x20 | (rem & 0x1f)) + 63));
                    rem >>= 5;
                }
                str.Append((char)(rem + 63));
            });

            int lastLat = 0;
            int lastLng = 0;
            foreach (var point in points)
            {
                int lat = (int)Math.Round(point.Latitude * 1E5);
                int lng = (int)Math.Round(point.Longitude * 1E5);
                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);
                lastLat = lat;
                lastLng = lng;
            }
            return str.ToString();
        }
    }
}
