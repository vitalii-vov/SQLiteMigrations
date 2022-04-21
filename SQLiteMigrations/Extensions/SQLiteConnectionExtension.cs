using System.Threading.Tasks;
using SQLite;

namespace SQLiteMigrations.Extensions
{
    internal static class SQLiteConnectionExtension
    {
        public static Task<int> GetPragmaVersionAsync(this SQLiteAsyncConnection connection)
        {
            return connection.ExecuteScalarAsync<int>("PRAGMA user_version");
        }

        public static Task SetPragmaVersionAsync(this SQLiteAsyncConnection connection, int version)
        {
            return connection.ExecuteAsync($"PRAGMA user_version = {version}");
        }

        public static int GetPragmaVersion(this SQLiteConnection connection)
        {
            return connection.ExecuteScalar<int>("PRAGMA user_version");
        }

        public static void SetPragmaVersion(this SQLiteConnection connection, int version)
        {
            connection.Execute($"PRAGMA user_version = {version}");
        }
    }
}
