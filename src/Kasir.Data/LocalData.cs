using System.Globalization;
using Kasir.Core;

namespace Kasir.Data;

public sealed class LocalData
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly object _tx = new();
    private const string Dt = "o";

    public void Transact(Action work)
    {
        lock (_tx) work();
    }

    public static string DataDir() => Path.Combine(KasirDbContext.DataDirectory(), "data");

    public CsvStore<Category> Categories { get; }
    public CsvStore<Product> Products { get; }
    public CsvStore<User> Users { get; }
    public CsvStore<Sale> Sales { get; }
    public CsvStore<SaleItem> SaleItems { get; }
    public CsvStore<StockMovement> StockMovements { get; }
    public CsvStore<DailySequence> DailySequences { get; }

    public LocalData(string? dir = null)
    {
        var d = dir ?? DataDir();
        Categories = new CsvStore<Category>(Path.Combine(d, "categories.csv"),
            new[] { "Id", "Name" },
            c => new[] { c.Id.ToString(Inv), c.Name },
            r => r.Length < 2 ? null : new Category { Id = int.Parse(r[0], Inv), Name = r[1] });
        Products = new CsvStore<Product>(Path.Combine(d, "products.csv"),
            new[] { "Id", "Barcode", "Name", "CategoryId", "PurchasePrice", "SellPrice", "Stock", "Unit", "MinStock", "IsActive", "CreatedAt", "UpdatedAt" },
            p => new[] {
                p.Id.ToString(Inv), p.Barcode, p.Name,
                p.CategoryId.HasValue ? p.CategoryId.Value.ToString(Inv) : string.Empty,
                p.PurchasePrice.ToString(Inv), p.SellPrice.ToString(Inv),
                p.Stock.ToString(Inv), p.Unit, p.MinStock.ToString(Inv),
                p.IsActive ? "1" : "0",
                p.CreatedAt.ToString(Dt, Inv), p.UpdatedAt.ToString(Dt, Inv) },
            r => r.Length < 12 ? null : new Product {
                Id = int.Parse(r[0], Inv), Barcode = r[1], Name = r[2],
                CategoryId = string.IsNullOrEmpty(r[3]) ? null : int.Parse(r[3], Inv),
                PurchasePrice = decimal.Parse(r[4], Inv), SellPrice = decimal.Parse(r[5], Inv),
                Stock = double.Parse(r[6], Inv), Unit = r[7], MinStock = double.Parse(r[8], Inv),
                IsActive = r[9] == "1",
                CreatedAt = DateTime.Parse(r[10], Inv, DateTimeStyles.RoundtripKind),
                UpdatedAt = DateTime.Parse(r[11], Inv, DateTimeStyles.RoundtripKind) });
        Users = new CsvStore<User>(Path.Combine(d, "users.csv"),
            new[] { "Id", "Username", "PasswordHash", "FullName", "Role", "IsActive" },
            u => new[] {
                u.Id.ToString(Inv), u.Username, u.PasswordHash, u.FullName,
                ((int)u.Role).ToString(Inv), u.IsActive ? "1" : "0" },
            r => r.Length < 6 ? null : new User {
                Id = int.Parse(r[0], Inv), Username = r[1], PasswordHash = r[2],
                FullName = r[3], Role = (UserRole)int.Parse(r[4], Inv), IsActive = r[5] == "1" });
        Sales = new CsvStore<Sale>(Path.Combine(d, "sales.csv"),
            new[] { "Id", "InvoiceNo", "CashierId", "Subtotal", "Discount", "Tax", "Total", "Paid", "Change", "PaymentMethod", "CreatedAt", "Note" },
            s => new[] {
                s.Id.ToString(Inv), s.InvoiceNo, s.CashierId.ToString(Inv),
                s.Subtotal.ToString(Inv), s.Discount.ToString(Inv), s.Tax.ToString(Inv),
                s.Total.ToString(Inv), s.Paid.ToString(Inv), s.Change.ToString(Inv),
                s.PaymentMethod, s.CreatedAt.ToString(Dt, Inv), s.Note ?? string.Empty },
            r => r.Length < 12 ? null : new Sale {
                Id = int.Parse(r[0], Inv), InvoiceNo = r[1], CashierId = int.Parse(r[2], Inv),
                Subtotal = decimal.Parse(r[3], Inv), Discount = decimal.Parse(r[4], Inv),
                Tax = decimal.Parse(r[5], Inv), Total = decimal.Parse(r[6], Inv),
                Paid = decimal.Parse(r[7], Inv), Change = decimal.Parse(r[8], Inv),
                PaymentMethod = r[9],
                CreatedAt = DateTime.Parse(r[10], Inv, DateTimeStyles.RoundtripKind),
                Note = string.IsNullOrEmpty(r[11]) ? null : r[11] });
        SaleItems = new CsvStore<SaleItem>(Path.Combine(d, "sale_items.csv"),
            new[] { "Id", "SaleId", "ProductId", "Qty", "Price", "Discount", "Subtotal" },
            i => new[] {
                i.Id.ToString(Inv), i.SaleId.ToString(Inv), i.ProductId.ToString(Inv),
                i.Qty.ToString(Inv), i.Price.ToString(Inv),
                i.Discount.ToString(Inv), i.Subtotal.ToString(Inv) },
            r => r.Length < 7 ? null : new SaleItem {
                Id = int.Parse(r[0], Inv), SaleId = int.Parse(r[1], Inv),
                ProductId = int.Parse(r[2], Inv), Qty = double.Parse(r[3], Inv),
                Price = decimal.Parse(r[4], Inv), Discount = decimal.Parse(r[5], Inv),
                Subtotal = decimal.Parse(r[6], Inv) });
        StockMovements = new CsvStore<StockMovement>(Path.Combine(d, "stock_movements.csv"),
            new[] { "Id", "ProductId", "QtyChange", "Type", "RefNo", "Note", "CreatedAt" },
            m => new[] {
                m.Id.ToString(Inv), m.ProductId.ToString(Inv), m.QtyChange.ToString(Inv),
                ((int)m.Type).ToString(Inv), m.RefNo ?? string.Empty, m.Note ?? string.Empty,
                m.CreatedAt.ToString(Dt, Inv) },
            r => r.Length < 7 ? null : new StockMovement {
                Id = int.Parse(r[0], Inv), ProductId = int.Parse(r[1], Inv),
                QtyChange = double.Parse(r[2], Inv), Type = (StockMovementType)int.Parse(r[3], Inv),
                RefNo = string.IsNullOrEmpty(r[4]) ? null : r[4],
                Note = string.IsNullOrEmpty(r[5]) ? null : r[5],
                CreatedAt = DateTime.Parse(r[6], Inv, DateTimeStyles.RoundtripKind) });
        DailySequences = new CsvStore<DailySequence>(Path.Combine(d, "daily_sequences.csv"),
            new[] { "Id", "Day", "LastNumber" },
            s => new[] { s.Id.ToString(Inv), s.Day, s.LastNumber.ToString(Inv) },
            r => r.Length < 3 ? null : new DailySequence {
                Id = int.Parse(r[0], Inv), Day = r[1], LastNumber = int.Parse(r[2], Inv) });
    }

    public void LoadAll()
    {
        Categories.Load();
        Products.Load();
        Users.Load();
        Sales.Load();
        SaleItems.Load();
        StockMovements.Load();
        DailySequences.Load();
    }

    public static int NextId(IEnumerable<int> ids)
    {
        var m = 0;
        foreach (var i in ids) if (i > m) m = i;
        return m + 1;
    }

    public void SeedIfEmpty()
    {
        if (!Categories.All().Any())
            Categories.ReplaceAll(new List<Category> { new() { Id = 1, Name = "Umum" } });
        if (!Users.All().Any())
            Users.ReplaceAll(new List<User> { new() { Id = 1, Username = "admin", PasswordHash = "admin", FullName = "Administrator", Role = UserRole.Admin, IsActive = true } });
        if (!Products.All().Any())
            Products.ReplaceAll(new List<Product>
            {
                new() { Id = 1, Barcode = "8991001", Name = "Indomie Goreng", CategoryId = 1, PurchasePrice = 2500, SellPrice = 3500, Stock = 100, Unit = "pcs" },
                new() { Id = 2, Barcode = "8991002", Name = "Aqua 600ml", CategoryId = 1, PurchasePrice = 2000, SellPrice = 3000, Stock = 120, Unit = "pcs" },
                new() { Id = 3, Barcode = "8991003", Name = "Kopi Kapal Api 25g", CategoryId = 1, PurchasePrice = 1500, SellPrice = 2500, Stock = 80, Unit = "pcs" },
                new() { Id = 4, Barcode = "8991004", Name = "Beras 1kg", CategoryId = 1, PurchasePrice = 12000, SellPrice = 14000, Stock = 50, Unit = "pcs" },
                new() { Id = 5, Barcode = "8991005", Name = "Gula Pasir 1kg", CategoryId = 1, PurchasePrice = 13000, SellPrice = 15000, Stock = 40, Unit = "pcs" },
            });
    }
}
