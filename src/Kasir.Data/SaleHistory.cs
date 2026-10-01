using Kasir.Core;
using Microsoft.EntityFrameworkCore;

namespace Kasir.Data;

public sealed record SaleRow(
    int Id,
    string InvoiceNo,
    decimal Total,
    decimal Paid,
    decimal Change,
    string PaymentMethod,
    DateTime CreatedAt,
    string CashierName);

public sealed record SaleItemRow(
    int ProductId,
    string ProductName,
    string Barcode,
    double Qty,
    decimal Price,
    decimal Discount,
    decimal Subtotal);

public sealed record SaleDetail(
    int Id,
    string InvoiceNo,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    decimal Paid,
    decimal Change,
    string PaymentMethod,
    DateTime CreatedAt,
    string CashierName,
    string? Note,
    List<SaleItemRow> Items);

public static class SaleHistory
{
    public static List<SaleRow> Search(
        KasirDbContext db,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        string? invoiceFilter,
        int limit = 200)
    {
        var q = db.Sales.AsNoTracking()
            .Where(s => s.CreatedAt >= fromUtc && s.CreatedAt < toUtcExclusive);
        var f = (invoiceFilter ?? string.Empty).Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(f))
            q = q.Where(s => s.InvoiceNo.ToLower().Contains(f));
        return q.OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id)
            .Take(Math.Clamp(limit, 1, 1000))
            .Select(s => new SaleRow(
                s.Id,
                s.InvoiceNo,
                s.Total,
                s.Paid,
                s.Change,
                s.PaymentMethod,
                s.CreatedAt,
                s.Cashier != null ? s.Cashier.FullName : "-"))
            .ToList();
    }

    public static SaleDetail? GetDetail(KasirDbContext db, int saleId)
    {
        var s = db.Sales.AsNoTracking()
            .Include(x => x.Items).ThenInclude(i => i.Product)
            .Include(x => x.Cashier)
            .FirstOrDefault(x => x.Id == saleId);
        if (s is null) return null;
        return new SaleDetail(
            s.Id,
            s.InvoiceNo,
            s.Subtotal,
            s.Discount,
            s.Tax,
            s.Total,
            s.Paid,
            s.Change,
            s.PaymentMethod,
            s.CreatedAt,
            s.Cashier != null ? s.Cashier.FullName : "-",
            s.Note,
            s.Items.OrderBy(i => i.Id).Select(i => new SaleItemRow(
                i.ProductId,
                i.Product != null ? i.Product.Name : $"Produk #{i.ProductId}",
                i.Product != null ? i.Product.Barcode : "-",
                i.Qty,
                i.Price,
                i.Discount,
                i.Subtotal)).ToList());
    }
}
