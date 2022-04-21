using System;
namespace SQLiteMigrations.Exceptions
{
    public class MigrationMissingException : Exception
    {
        public MigrationMissingException(int oldVersion)
            : base($"Migration from version {oldVersion} not found")
        {
        }
    }
}
