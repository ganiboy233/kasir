namespace Kasir.Core;

public class Product
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellPrice { get; set; }
    public double Stock { get; set; }
    public string Unit { get; set; } = "pcs";
    public double MinStock { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
