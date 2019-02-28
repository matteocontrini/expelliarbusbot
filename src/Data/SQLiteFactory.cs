using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace Data
{
    public class SQLiteFactory : ISQLiteFactory
    {
        private readonly Dictionary<string, SQLiteAsyncConnection> cache;

        public SQLiteFactory(DatabaseConfiguration conf)
        {
            this.cache = new Dictionary<string, SQLiteAsyncConnection>();

            // Create GTFS data connection
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), conf.GtfsDataPath);

            if (!File.Exists(dbPath))
            {
                throw new Exception($"GTFS database not found in {dbPath}");
            }

            this.cache["gtfs"] = new SQLiteAsyncConnection(dbPath);

            // Create BOT data connection
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), conf.BotDataPath);

            if (!File.Exists(dbPath))
            {
                throw new Exception($"BOT database not found in {dbPath}");
            }

            this.cache["bot"] = new SQLiteAsyncConnection(dbPath);
        }

        public SQLiteAsyncConnection GetConnection(string key)
        {
            return this.cache[key];
        }
    }
}
