## SQLiteMigrations

SQLiteMigrations is a .NET Standard Library for use with [SQLite-net](https://github.com/praeclarum/sqlite-net) that allows you to manage database migrations using the PRAGMA option

## Dependencies

[SQLite-net](https://github.com/praeclarum/sqlite-net)

## NuGet Installation

Install [Vitvov.SQLiteMigrations](https://www.nuget.org/packages/Vitvov.SQLiteMigrations) from NuGet.

## Usage

### Create a database connection

Let's say you have a database table

```csharp
[Table("record")]
public class RecordEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public long Id { get; set; }

    [Column("startTime")]
    public DateTime StartTime { get; set; }

    [Column("endTime")]
    public DateTime EndTime { get; set; }
}
```
In order to create a database, you must create a class that implements `SQLiteAsyncDatabase`.
You will have access to the public property `Connection` of type `SQLiteAsyncConnection`.
In the `CreateAsync()` method, write the database creation script.
Return `null` in the `Migrations()` method.
```csharp
public class AppDatabase : SQLiteAsyncDatabase
{
    private const int DB_VERSION = 1;

    public AppDatabase(string path) : base(DB_VERSION, path)
    {
    }

    public override async Task CreateAsync()
    {
        await Connection.CreateTableAsync<RecordEntity>();
    }

    public override List<IMigration> Migrations()
    {
        return null;
    }
}
```
To access tables, use the `Connection` property. For example, you can add a property to your class
```csharp
public class AppDatabase : SQLiteAsyncDatabase
{
    // some code ...
    public AsyncTableQuery<RecordEntity> RecordsTable => Connection.Table<RecordEntity>();
}
```
To start working with the AppDatabase, call
```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "db.sqlite");
var database = new AppDatabase(dbPath);
await database.ConnectAsync();
//usage
//database.Connection.Table<RecordEntity>()...
```

### Add migrations

Let's say you want to add a new table
```csharp
[Table("data")]
public class DataEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public long Id { get; set; }

    [Column("pulse")]
    public int Pulse { get; set; }

    [Column("recordId")]
    public long RecordId { get; set; }
}
```
Let's create a migration for our database. Create a new migration class and implement the `IMigration` interface
```csharp
public class Migration1To2 : IMigration
{
    public int OldVersion => 1;
    public int NewVersion => 2;

    public void Migrate(SQLiteConnection connection)
    {
        connection.CreateTable<DataEntity>();
    }
}
```
And also make a change to the database class
```csharp
public class AppDatabase : SQLiteAsyncDatabase
{
    private const int DB_VERSION = 2; // <- change version from 1 to 2

    public AppDatabase(string path) : base(DB_VERSION, path)
    {
    }

    public override async Task CreateAsync()
    {
        await Connection.CreateTableAsync<RecordEntity>();
        await Connection.CreateTable<DataEntity>(); // <- add line
    }

    public override List<IMigration> Migrations()
    {
        // <- return the migration
        return new List<IMigration>
        {
            new Migration1To2()
        };
    }
}
```

### One more example

Let's say you want to add a new field to the RecordEntity table 

```csharp
[Table("record")]
public class RecordEntity
{
    // some code...
    
    //  new property
    [Column("recorder")]
    public string RecorderName { get; set; }
}
```
Let's create a migration for our database

```csharp
public class Migration2To3 : IMigration
{
    public int OldVersion => 2;
    public int NewVersion => 3;

    public void Migrate(SQLiteConnection connection)
    {
        connection.Execute("ALTER TABLE record ADD COLUMN recorder VARCHAR");
    }
}
```

And also make a change to the database class
```csharp
public class AppDatabase : SQLiteAsyncDatabase
{
    private const int DB_VERSION = 3; // <- change version from 2 to 3

    public AppDatabase(string path) : base(DB_VERSION, path)
    {
    }

    public override async Task CreateAsync()
    {
        await Connection.CreateTableAsync<RecordEntity>();
        await Connection.CreateTable<DataEntity>();
    }

    public override List<IMigration> Migrations()
    {
        return new List<IMigration>
        {
            new Migration1To2(),
            new Migration2To3()// <- add new line
        };
    }
}
```

## Using own SQLiteConnectionString

To use your own connection string, pass it to the base class constructor.

```csharp
public class AppDatabase : SQLiteAsyncDatabase
{
    public AppDatabase(SQLiteConnectionString сonnectionString) : base(DB_VERSION, сonnectionString)
    {
    }
}
```

```csharp
var databasePath = Path.Combine(FileSystem.AppDataDirectory, "db.sqlite");
var options = new SQLiteConnectionString(databasePath, true, key: "password");
var database = new AppDatabase(options);
await database.ConnectAsync();
```
