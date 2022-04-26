using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteMigrations.Exceptions;
using SQLiteMigrations.Extensions;

namespace SQLiteMigrations
{
    public abstract class SQLiteAsyncDatabase
    {
        private readonly int _version;
        private readonly SQLiteConnectionString _connectionString;

        protected SQLiteAsyncDatabase(int version, SQLiteConnectionString connectionString)
        {
            if (version <= 0)
                throw new ArgumentException("The database version must not be less than or equal to 0");

            _version = version;
            _connectionString = connectionString;
        }

        protected SQLiteAsyncDatabase(int version, string path)
            : this(version, new SQLiteConnectionString(path))
        {
        }

        public SQLiteAsyncConnection Connection { get; private set; }

        public abstract Task CreateAsync();

        public abstract List<IMigration> Migrations();

        public async Task ConnectAsync()
        {
            Connection = new SQLiteAsyncConnection(_connectionString);
            await CreateDatabaseAsync();
            await MigrateDatabaseAsync();
        }

        public async Task CloseAsync()
        {
            await Connection.CloseAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            //  Was created early
            if (await Connection.GetPragmaVersionAsync() != 0)
                return;

            //  First time create database
            await CreateAsync();
            await Connection.SetPragmaVersionAsync(_version);
        }

        private async Task MigrateDatabaseAsync()
        {
            var currentVersion = await Connection.GetPragmaVersionAsync();

            //  If is current version - not need migration
            if (currentVersion == _version)
                return;

            var migrations = Migrations();

            //  We take only those migrations, the versions of which are later than the current version of the database
            migrations = FilterMigrations(migrations, currentVersion);

            ThrowIfMissingFirstMigration(migrations, currentVersion);
            ThrowIfMissingTargetMigration(migrations, _version);
            ThrowIfMigrationsAreMissing(migrations);

            await Connection.RunInTransactionAsync(conn =>
            {
                foreach (var migration in migrations.OrderBy(a => a.OldVersion))
                {
                    migration.Migrate(conn);
                    conn.SetPragmaVersion(migration.NewVersion);
                }
            });
        }

        private List<IMigration> FilterMigrations(List<IMigration> migrations, int currentVersion)
        {
            if (migrations is null)
                throw new MigrationMissingException(currentVersion);

            return migrations.Where(a => a.OldVersion >= currentVersion).ToList();
        }

        private void ThrowIfMissingFirstMigration(List<IMigration> migrations, int currentVersion)
        {
            if (!migrations.Exists(a => a.OldVersion == currentVersion))
                throw new MigrationMissingException(currentVersion);
        }

        private void ThrowIfMissingTargetMigration(List<IMigration> migrations, int targetVersion)
        {
            if (!migrations.Exists(a => a.NewVersion == targetVersion))
                throw new MigrationMissingTargetException(targetVersion);
        }

        private void ThrowIfMigrationsAreMissing(List<IMigration> migrations)
        {
            migrations = migrations.OrderBy(a => a.OldVersion).ToList();
            var lastOldVersion = migrations.First().OldVersion;
            foreach (var migration in migrations)
            {
                if (migration.OldVersion != lastOldVersion)
                    throw new MigrationMissingException(lastOldVersion);
                lastOldVersion = migration.NewVersion;
            }
        }
    }
}
