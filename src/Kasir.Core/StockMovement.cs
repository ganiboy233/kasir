namespace Kasir.Core;

public enum StockMovementType
{
    In,
    Out,
    Adjustment,
    Sale
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public double QtyChange { get; set; }
    public StockMovementType Type { get; set; }
    public string? RefNo { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
