using CommunityToolkit.Mvvm.ComponentModel;

namespace Kasir.Desktop.ViewModels;

public partial class CartLine : ObservableObject
{
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private double _qty = 1;

    public decimal Subtotal => Price * (decimal)Qty;

    partial void OnPriceChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
    partial void OnQtyChanged(double value) => OnPropertyChanged(nameof(Subtotal));
}
