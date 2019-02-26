using SQLite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MapsGenerator
{
    /// <summary>
    /// Console application to generate maps once using MapBox.
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            string databaseFilePath = @"C:\Users\mcont\Desktop\google_transit_urbano_tte\test.db";
            string token = ""; // mapbox public token

            var db = new SQLiteConnection(databaseFilePath);

            // TODO: get shapes dynamically from the database
            string[] shapes = new string[]
            {
                "D306_F0512_Ritorno_sub2",
                "D1173_T0526a_Ritorno_sub1",
                "D1190_T0542_Ritorno_sub1",
                "D1165_T0522g_Ritorno_sub1",
                "D1169_T0522o_Ritorno_sub2"
            };

            foreach (string shapeId in shapes)
            {
                Console.WriteLine(shapeId);
                Console.WriteLine();

                var points = db.Table<ShapePoint>()
                               .Where(x => x.ShapeId == shapeId)
                               .OrderBy(x => x.ShapeId)
                               .ToList();

                string polyline = Encode(points);
                string encoded = WebUtility.UrlEncode(polyline);

                //Console.WriteLine(encoded);

                string url = $"https://api.mapbox.com/styles/v1/matteocontrini/cjslp4zzs5fef1fpeowgokfff/static/path-5+f5c500({encoded}),pin-l-a+E94335(11.150372,46.067348),pin-l-b+E94335(11.154596,46.065955),pin-l-c+E94335(11.151912,46.063862),pin-l-e+E94335(11.150209,46.063316),pin-l-d+E94335(11.150560,46.063947),pin-l-f+E94335(11.146326,46.065746)/11.1507,46.0653,15.3,0,0/600x600@2x?access_token={token}";

                Console.WriteLine(url);
                Console.WriteLine();

                using (var client = new WebClient())
                {
                    // Uncomment to download and save map images
                    //client.DownloadFile(url, $"{shapeId}.png");
                }
            }
            
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
