using System;
namespace SQLiteMigrations.Exceptions
{
    public class MigrationMissingTargetException : Exception
    {
        public MigrationMissingTargetException(int newVersion)
            : base($"Migration to version {newVersion} not found")
        {
        }
    }
}
