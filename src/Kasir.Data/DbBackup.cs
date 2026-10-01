using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kasir.Data;

public sealed record BackupResult(bool Ok, string Message, string? Path);

public static class DbBackup
{
    public const int KeepCount = 7;
    private static readonly string[] RequiredTables = { "Sales", "Products", "Users", "Categories", "DailySequences" };

    public static string BackupDirectory(string? dir = null)
    {
        var d = dir ?? Path.Combine(KasirDbContext.DataDirectory(), "backups");
        Directory.CreateDirectory(d);
        return d;
    }

    public static List<string> ListBackups(string? dir = null) =>
        Directory.GetFiles(BackupDirectory(dir), "cashier-exhaust-*.db").OrderDescending().ToList();

    public static BackupResult CreateBackup(string? sourcePath = null, string? backupDir = null)
    {
        try
        {
            var src = sourcePath ?? KasirDbContext.DefaultDbPath();
            if (!File.Exists(src))
                return new BackupResult(false, "DB belum ada, tidak ada yang dibackup.", null);
            var dest = UniquePath(BackupDirectory(backupDir), $"cashier-exhaust-{DateTime.Now:yyyyMMdd-HHmmss}.db");
            // VACUUM INTO aman terhadap koneksi pool yang masih terbuka,
            // File.Copy mentah tidak aman dan terbukti bisa menyalin tabel kosong.
            var opt = new DbContextOptionsBuilder<KasirDbContext>().UseSqlite($"Data Source={src}").Options;
            using (var db = new KasirDbContext(opt))
            {
                // VACUUM INTO tidak mendukung parameter, path sudah di-escape manual.
#pragma warning disable EF1002
                db.Database.ExecuteSqlRaw($"VACUUM INTO '{dest.Replace("'", "''")}';");
#pragma warning restore EF1002
            }
            var check = VerifyBackupFile(dest);
            if (!check.Ok)
            {
                SqliteConnection.ClearAllPools();
                try { File.Delete(dest); } catch { }
                return new BackupResult(false, $"Backup gagal verifikasi: {check.Message}", null);
            }
            Prune(backupDir);
            return new BackupResult(true, $"Backup tersimpan: {Path.GetFileName(dest)}", dest);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, $"Backup gagal: {ex.Message}", null);
        }
    }

    public static BackupResult RestoreBackup(string backupPath, string? targetPath = null)
    {
        try
        {
            if (!File.Exists(backupPath))
                return new BackupResult(false, "File backup tidak ditemukan.", null);
            var check = VerifyBackupFile(backupPath);
            if (!check.Ok)
                return new BackupResult(false, $"File backup rusak: {check.Message}", null);
            SqliteConnection.ClearAllPools();
            var dest = targetPath ?? KasirDbContext.DefaultDbPath();
            File.Copy(backupPath, dest, overwrite: true);
            return new BackupResult(true, "Restore selesai. Tutup dan buka ulang aplikasi.", dest);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, $"Restore gagal: {ex.Message}", null);
        }
    }

    public static BackupResult QuickCheck(string dbPath)
    {
        try
        {
            var opt = new DbContextOptionsBuilder<KasirDbContext>()
                .UseSqlite($"Data Source={dbPath};Mode=ReadOnly")
                .Options;
            using var db = new KasirDbContext(opt);
            var conn = db.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA quick_check;";
            var result = cmd.ExecuteScalar()?.ToString();
            if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                return new BackupResult(false, $"quick_check: {result}", null);
            return new BackupResult(true, "ok", dbPath);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, ex.Message, null);
        }
    }

    public static BackupResult VerifyBackupFile(string dbPath)
    {
        var q = QuickCheck(dbPath);
        if (!q.Ok) return q;
        try
        {
            var opt = new DbContextOptionsBuilder<KasirDbContext>()
                .UseSqlite($"Data Source={dbPath};Mode=ReadOnly")
                .Options;
            using var db = new KasirDbContext(opt);
            var names = db.Database.SqlQueryRaw<string>("SELECT name FROM sqlite_master WHERE type='table'").ToList();
            var missing = RequiredTables.Where(t => !names.Contains(t)).ToList();
            if (missing.Count > 0)
                return new BackupResult(false, "Tabel hilang: " + string.Join(",", missing), null);
            return new BackupResult(true, "ok", dbPath);
        }
        catch (Exception ex)
        {
            return new BackupResult(false, ex.Message, null);
        }
    }

    private static string UniquePath(string dir, string file)
    {
        var dest = Path.Combine(dir, file);
        var i = 1;
        while (File.Exists(dest))
        {
            dest = Path.Combine(dir, Path.GetFileNameWithoutExtension(file) + $"-{i}" + Path.GetExtension(file));
            i++;
        }
        return dest;
    }

    private static void Prune(string? dir = null)
    {
        // Pool SQLite menahan handle file. Tanpa ini, File.Delete gagal diam-diam di Windows.
        SqliteConnection.ClearAllPools();
        foreach (var old in ListBackups(dir).Skip(KeepCount))
        {
            try { File.Delete(old); } catch { }
        }
    }
}
