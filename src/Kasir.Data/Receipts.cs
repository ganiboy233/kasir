using Kasir.Core;

namespace Kasir.Data;

public static class Receipts
{
    public static ReceiptModel FromSaleDetail(StoreProfile store, SaleDetail d, bool isCopy = false) => new(
        new ReceiptHeader(
            store.Name, store.Address, store.Phone,
            d.InvoiceNo, d.CreatedAt, d.CashierName, d.PaymentMethod),
        d.Items.Select(i => new ReceiptLine(i.ProductName, i.Qty, i.Price, i.Discount, i.Subtotal)).ToList(),
        new ReceiptTotals(d.Subtotal, d.Discount, d.Tax, d.Total, d.Paid, d.Change),
        store.Footer,
        isCopy);
}
