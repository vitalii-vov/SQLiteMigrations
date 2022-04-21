using SQLite;

namespace SQLiteMigrations
{
    public interface IMigration
    {
        int OldVersion { get; }
        int NewVersion { get; }

        void Migrate(SQLiteConnection connection);
    }
}
