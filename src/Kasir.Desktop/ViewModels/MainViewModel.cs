using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kasir.Core;
using Kasir.Data;

namespace Kasir.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private CancellationTokenSource? _searchCts;
    private const int SearchDebounceMs = 200;
    private readonly LocalData _data = new();
    [ObservableProperty] private string _barcodeInput = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusMessage = "Siap. Scan barcode atau cari produk.";
    [ObservableProperty] private decimal _paidAmount;
    [ObservableProperty] private decimal _total;

    public ObservableCollection<CartLine> Cart { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();

    public decimal Change => PaidAmount - Total;

    public MainViewModel()
    {
        InitStore();
        RefreshProducts(SearchText);
        Cart.CollectionChanged += (_, _) => Recalc();
        PaidAmount = 0;
    }

    private void InitStore()
    {
        try
        {
            _data.LoadAll();
            _data.SeedIfEmpty();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Data error: {ex.Message}";
        }
    }

    private void RefreshProducts(string? filter)
    {
        try
        {
            var f = Barcode.NormalizeFilter(filter);
            var q = _data.Products.All().Where(p => p.IsActive);
            if (!string.IsNullOrEmpty(f))
                q = q.Where(p => p.Name.ToLowerInvariant().Contains(f) || p.Barcode.ToLowerInvariant().Contains(f));
            Products.Clear();
            foreach (var p in q.OrderBy(p => p.Name).Take(100).ToList())
                Products.Add(p);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Gagal load produk: {ex.Message}";
        }
    }

    partial void OnSearchTextChanged(string value) => DebounceSearch(value);

    private void DebounceSearch(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        var cts = new CancellationTokenSource();
        _searchCts = cts;
        var captured = value;
        Task.Delay(SearchDebounceMs, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            Dispatcher.UIThread.Post(() =>
            {
                if (cts.IsCancellationRequested) return;
                RefreshProducts(captured);
            });
        }, TaskScheduler.Default);
    }
    partial void OnPaidAmountChanged(decimal value) => OnPropertyChanged(nameof(Change));

    private void Recalc()
    {
        foreach (var _ in Cart) { }
        Total = Cart.Sum(c => c.Subtotal);
        OnPropertyChanged(nameof(Change));
    }

    [RelayCommand]
    private void AddByBarcode()
    {
        var code = Barcode.Normalize(BarcodeInput);
        if (string.IsNullOrEmpty(code)) return;
        if (!Barcode.IsPlausible(code))
        {
            StatusMessage = $"Barcode tidak valid: {code}.";
            BeepError();
            return;
        }
        try
        {
            var p = _data.Products.All().FirstOrDefault(x => x.Barcode == code);
            if (p is null)
            {
                StatusMessage = $"Barcode {code} tidak ada. Cek fisik barang atau tambah produk.";
                BeepError();
                return;
            }
            if (!p.IsActive)
            {
                StatusMessage = $"{p.Name} nonaktif. Aktifkan dulu di data produk.";
                BeepError();
                return;
            }
            AddToCart(p);
            BarcodeInput = string.Empty;
            BeepOk();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error scan: {ex.Message}";
        }
    }

    private static void BeepOk()
    {
        if (!OperatingSystem.IsWindows()) return;
        try { Console.Beep(880, 80); } catch { }
    }

    private static void BeepError()
    {
        if (!OperatingSystem.IsWindows()) return;
        try { Console.Beep(220, 150); } catch { }
    }

    [RelayCommand]
    private void AddProduct(Product? p)
    {
        if (p is null) return;
        AddToCart(p);
    }

    private void AddToCart(Product p)
    {
        var line = Cart.FirstOrDefault(c => c.ProductId == p.Id);
        if (line is null)
        {
            line = new CartLine { ProductId = p.Id, Barcode = p.Barcode, Name = p.Name, Price = p.SellPrice, Qty = 1 };
            line.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CartLine.Subtotal)) Recalc(); };
            Cart.Add(line);
        }
        else
        {
            line.Qty += 1;
        }
        StatusMessage = $"{p.Name} x{line.Qty} di keranjang.";
        Recalc();
    }

    [RelayCommand]
    private void Inc(CartLine? line)
    {
        if (line is null) return;
        line.Qty += 1;
        Recalc();
    }

    [RelayCommand]
    private void Dec(CartLine? line)
    {
        if (line is null) return;
        line.Qty -= 1;
        if (line.Qty <= 0) Cart.Remove(line);
        Recalc();
    }

    [RelayCommand]
    private void RemoveLine(CartLine? line)
    {
        if (line is null) return;
        Cart.Remove(line);
        Recalc();
    }

    [RelayCommand]
    private void SetExactPayment()
    {
        PaidAmount = Total;
    }

    [RelayCommand]
    private void ClearCart()
    {
        Cart.Clear();
        PaidAmount = 0;
        Recalc();
        StatusMessage = "Keranjang dikosongkan.";
    }

    [RelayCommand]
    private void Checkout()
    {
        if (Cart.Count == 0)
        {
            StatusMessage = "Keranjang kosong.";
            return;
        }
        if (PaidAmount < Total)
        {
            StatusMessage = $"Uang kurang. Total Rp {Total:N0}, bayar Rp {PaidAmount:N0}.";
            return;
        }
        var lines = Cart.ToList();
        try
        {
            var invoice = string.Empty;
            decimal kembalian = 0;
            _data.Transact(() =>
            {
                var products = _data.Products.All();
                var byId = products.ToDictionary(p => p.Id);
                foreach (var l in lines)
                {
                    if (!byId.TryGetValue(l.ProductId, out var pr))
                        throw new InvalidOperationException($"Produk {l.Name} sudah tidak ada di data. Muat ulang keranjang.");
                    if (pr.Stock < l.Qty)
                        throw new InvalidOperationException($"Stok {pr.Name} tinggal {pr.Stock}, keranjang minta {l.Qty}. Kurangi qty atau tambah stok.");
                }

                var dayKey = DateTime.Now.ToString("yyyyMMdd");
                var seqs = _data.DailySequences.All();
                var seq = seqs.FirstOrDefault(s => s.Day == dayKey);
                if (seq is null)
                {
                    seq = new DailySequence { Id = LocalData.NextId(seqs.Select(s => s.Id)), Day = dayKey, LastNumber = 0 };
                    seqs.Add(seq);
                }
                seq.LastNumber += 1;
                invoice = $"{AppInfo.InvoicePrefix}{dayKey}-{seq.LastNumber:0000}";

                var total = lines.Sum(l => l.Subtotal);
                var sale = new Sale
                {
                    Id = LocalData.NextId(_data.Sales.All().Select(s => s.Id)),
                    InvoiceNo = invoice,
                    CashierId = 1,
                    Subtotal = total,
                    Discount = 0,
                    Tax = 0,
                    Total = total,
                    Paid = PaidAmount,
                    Change = PaidAmount - total,
                    PaymentMethod = "Cash"
                };
                var sales = _data.Sales.All();
                sales.Add(sale);
                _data.Sales.ReplaceAll(sales);

                var items = _data.SaleItems.All();
                var moves = _data.StockMovements.All();
                var itemId = LocalData.NextId(items.Select(i => i.Id));
                var moveId = LocalData.NextId(moves.Select(m => m.Id));
                foreach (var l in lines)
                {
                    var pr = byId[l.ProductId];
                    pr.Stock -= l.Qty;
                    pr.UpdatedAt = DateTime.UtcNow;
                    items.Add(new SaleItem { Id = itemId++, SaleId = sale.Id, ProductId = pr.Id, Qty = l.Qty, Price = l.Price, Discount = 0, Subtotal = l.Subtotal });
                    moves.Add(new StockMovement { Id = moveId++, ProductId = pr.Id, QtyChange = -l.Qty, Type = StockMovementType.Sale, RefNo = invoice, Note = $"Penjualan {invoice}" });
                }
                _data.SaleItems.ReplaceAll(items);
                _data.StockMovements.ReplaceAll(moves);
                _data.Products.ReplaceAll(products);
                _data.DailySequences.ReplaceAll(seqs);
                kembalian = sale.Change;
            });
            Cart.Clear();
            PaidAmount = 0;
            Recalc();
            RefreshProducts(SearchText);
            StatusMessage = $"Sukses {invoice}. Kembalian Rp {kembalian:N0}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Checkout gagal, tidak ada stok yang berubah: {ex.Message}";
        }
    }
}
