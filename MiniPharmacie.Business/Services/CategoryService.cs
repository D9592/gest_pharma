using MiniPharmacie.Data.Database;

namespace MiniPharmacie.Business.Services;

public sealed class CategoryService(DatabaseInitializer db)
{
    public async Task<long> SaveAsync(string name,long actorId,long? id=null,CancellationToken ct=default){if(string.IsNullOrWhiteSpace(name))throw new ArgumentException("Le nom de la catégorie est obligatoire.");await using var c=db.CreateConnection();await c.OpenAsync(ct);await SecurityAdministrationService.RequireManager(c,actorId,ct);await using var cmd=c.CreateCommand();cmd.CommandText=id is null?"INSERT INTO Categories(Name) VALUES($n);SELECT last_insert_rowid()":"UPDATE Categories SET Name=$n WHERE Id=$id;SELECT $id";cmd.Parameters.AddWithValue("$n",name.Trim());if(id is not null)cmd.Parameters.AddWithValue("$id",id.Value);try{return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));}catch(Microsoft.Data.Sqlite.SqliteException ex) when(ex.SqliteErrorCode==19){throw new InvalidOperationException("Une catégorie de ce nom existe déjà.",ex);}}
    public async Task ArchiveAsync(long id,long actorId,CancellationToken ct=default){await using var c=db.CreateConnection();await c.OpenAsync(ct);await SecurityAdministrationService.RequireManager(c,actorId,ct);await using var cmd=c.CreateCommand();cmd.CommandText="UPDATE Categories SET IsActive=0 WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);await cmd.ExecuteNonQueryAsync(ct);}
}
