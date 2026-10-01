namespace Kasir.Core;

public sealed record ReceiptHeader(
    string StoreName,
    string Address,
    string Phone,
    string InvoiceNo,
    DateTime CreatedAt,
    string CashierName,
    string PaymentMethod);

public sealed record ReceiptLine(
    string Name,
    double Qty,
    decimal UnitPrice,
    decimal LineDiscount,
    decimal Subtotal);

public sealed record ReceiptTotals(
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    decimal Paid,
    decimal Change);

public sealed record ReceiptModel(
    ReceiptHeader Header,
    List<ReceiptLine> Lines,
    ReceiptTotals Totals,
    string Footer,
    bool IsCopy);
