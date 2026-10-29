using MiniPharmacie.Data.Database;

namespace MiniPharmacie.Business.Services;

public sealed class DailyBackupScheduler(DatabaseInitializer db,DatabaseBackupService backups)
{
    public async Task<string?> RunIfDueAsync(long managerId,CancellationToken ct=default){await using var c=db.CreateConnection();await c.OpenAsync(ct);await SecurityAdministrationService.RequireManager(c,managerId,ct);await using var q=c.CreateCommand();q.CommandText="SELECT Key,Value FROM PharmacySettings WHERE Key IN ('BackupTime','BackupDirectory','LastScheduledBackupDate')";var settings=new Dictionary<string,string>();await using(var r=await q.ExecuteReaderAsync(ct))while(await r.ReadAsync(ct))settings[r.GetString(0)]=r.GetString(1);if(!TimeOnly.TryParse(settings.GetValueOrDefault("BackupTime","23:00"),out var time))return null;var now=DateTime.Now;if(TimeOnly.FromDateTime(now)<time||settings.GetValueOrDefault("LastScheduledBackupDate")==DateOnly.FromDateTime(now).ToString("yyyy-MM-dd"))return null;var path=await backups.CreateVerifiedBackupAsync(managerId,settings.GetValueOrDefault("BackupDirectory"),ct);await using var update=c.CreateCommand();update.CommandText="INSERT INTO PharmacySettings(Key,Value) VALUES('LastScheduledBackupDate',$d) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value";update.Parameters.AddWithValue("$d",DateOnly.FromDateTime(now).ToString("yyyy-MM-dd"));await update.ExecuteNonQueryAsync(ct);return path;}
}
