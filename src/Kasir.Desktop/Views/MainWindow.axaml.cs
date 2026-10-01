using Avalonia.Controls;
using Avalonia.Input;
using Kasir.Desktop.ViewModels;

namespace Kasir.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BarcodeBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is MainViewModel vm && vm.AddByBarcodeCommand.CanExecute(null))
        {
            vm.AddByBarcodeCommand.Execute(null);
            e.Handled = true;
        }
    }
}
