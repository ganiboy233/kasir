using Kasir.Core;
using Microsoft.EntityFrameworkCore;

namespace Kasir.Data;

public class KasirDbContext : DbContext
{
    public KasirDbContext(DbContextOptions<KasirDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<DailySequence> DailySequences => Set<DailySequence>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>().HasIndex(p => p.Barcode).IsUnique();
        b.Entity<Product>().Property(p => p.PurchasePrice).HasPrecision(18, 2);
        b.Entity<Product>().Property(p => p.SellPrice).HasPrecision(18, 2);
        b.Entity<Sale>().HasIndex(s => s.InvoiceNo).IsUnique();
        b.Entity<Sale>().HasIndex(s => s.CreatedAt);
        b.Entity<DailySequence>().HasIndex(s => s.Day).IsUnique();
        b.Entity<Sale>().Property(s => s.Subtotal).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.Discount).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.Tax).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.Total).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.Paid).HasPrecision(18, 2);
        b.Entity<Sale>().Property(s => s.Change).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(i => i.Price).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(i => i.Discount).HasPrecision(18, 2);
        b.Entity<SaleItem>().Property(i => i.Subtotal).HasPrecision(18, 2);

        b.Entity<Category>().HasData(new Category { Id = 1, Name = "Umum" });
        b.Entity<User>().HasData(new User { Id = 1, Username = "admin", PasswordHash = "admin", FullName = "Administrator", Role = UserRole.Admin, IsActive = true });
    }

    public static string DataDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Kasir.Core.AppInfo.DataFolderName);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string DefaultDbPath()
    {
        return Path.Combine(DataDirectory(), Kasir.Core.AppInfo.DbFileName);
    }

    public static string LegacyDbPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kasir", "kasir.db");
    }

    public static string? TryMigrateLegacyDb()
    {
        try
        {
            var target = DefaultDbPath();
            if (File.Exists(target)) return null;
            var legacy = LegacyDbPath();
            if (!File.Exists(legacy)) return null;
            File.Copy(legacy, target);
            return $"Migrasi DB lama selesai: {legacy} -> {target}";
        }
        catch (Exception ex)
        {
            return $"Migrasi DB lama gagal: {ex.Message}";
        }
    }

    public static DbContextOptions<KasirDbContext> DefaultOptions(string? path = null)
    {
        var dbPath = path ?? DefaultDbPath();
        return new DbContextOptionsBuilder<KasirDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
    }
}
