namespace Kasir.Core;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public double Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Subtotal { get; set; }
}
