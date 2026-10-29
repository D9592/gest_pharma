using Microsoft.Data.Sqlite;

namespace MiniPharmacie.Data.Database;

public sealed class DatabaseInitializer(string? databasePath = null)
{
    public string DatabasePath { get; } = databasePath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MiniPharmacie", "pharmacie.db");

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath, ForeignKeys = true, Pooling = true }.ToString());
        return connection;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL;";
        await pragma.ExecuteNonQueryAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction();
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = Schema.V1;
        await command.ExecuteNonQueryAsync(cancellationToken);
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        var version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 2)
        {
            command.CommandText = Schema.V2;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(2,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 3)
        {
            command.CommandText = Schema.V3;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(3,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 4)
        {
            command.CommandText = Schema.V4;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "UPDATE SaleLines SET ProductName=(SELECT Name FROM Products WHERE Products.Id=SaleLines.ProductId) WHERE ProductName=''";
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(4,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 5)
        {
            command.CommandText = Schema.V5;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(5,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 6)
        {
            command.CommandText = Schema.V6;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(6,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        command.CommandText = "SELECT COALESCE(MAX(Version),0) FROM SchemaVersions";
        version = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        if (version < 7)
        {
            command.CommandText = Schema.V7;
            await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText = "INSERT INTO SchemaVersions(Version,AppliedAt) VALUES(7,datetime('now'))";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }
}
