using Automind.Treinamentos.Services;
using Microsoft.Data.Sqlite;

namespace Automind.Treinamentos.Data;

public sealed class TrainingDb
{
    private readonly StorageService _storage;

    public TrainingDb(StorageService storage)
    {
        _storage = storage;
    }

    public string DatabasePath => Path.Combine(_storage.DataPath, "Automind.Treinamentos.db");

    public SqliteConnection OpenConnection()
    {
        _storage.EnsureDirectories();
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
        var connection = new SqliteConnection(cs);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
