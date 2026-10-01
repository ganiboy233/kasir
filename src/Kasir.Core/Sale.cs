namespace Kasir.Core;

public class Sale
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int CashierId { get; set; }
    public User? Cashier { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Change { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public List<SaleItem> Items { get; set; } = new();
}
