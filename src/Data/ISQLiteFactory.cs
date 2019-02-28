using SQLite;

namespace Data
{
    public interface ISQLiteFactory
    {
        SQLiteAsyncConnection GetConnection(string key);
    }
}
