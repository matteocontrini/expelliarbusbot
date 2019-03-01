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

            this.cache["gtfs"] = new SQLiteAsyncConnection(
                databasePath: dbPath,
                // open in read only mode
                openFlags: SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex
            );

            // Create BOT data connection
            dbPath = Path.Combine(Directory.GetCurrentDirectory(), conf.BotDataPath);
            this.cache["bot"] = new SQLiteAsyncConnection(dbPath, storeDateTimeAsTicks: false);
            this.cache["bot"].CreateTableAsync<ChatEntity>();
        }

        public SQLiteAsyncConnection GetConnection(string key)
        {
            return this.cache[key];
        }
    }
}
