using Microsoft.Data.Sqlite;
using MiniPharmacie.Data.Database;

namespace MiniPharmacie.Tests;

public class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializesSchemaWithForeignKeysEnabled()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mini-pharmacie-{Guid.NewGuid():N}.db");
        try
        {
            var initializer = new DatabaseInitializer(path);
            await initializer.InitializeAsync();
            await using var connection = initializer.CreateConnection();
            await connection.OpenAsync();
            await using var cmd = connection.CreateCommand(); cmd.CommandText = "PRAGMA foreign_keys";
            Assert.Equal(1L, await cmd.ExecuteScalarAsync());
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Sales'";
            Assert.Equal(1L, await cmd.ExecuteScalarAsync());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            foreach (var suffix in new[] { "", "-wal", "-shm" })
                if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
    }
}
