using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Amazon.Models;
using Amazon.Services;
using Microsoft.Maui.Controls;

namespace Amazon.ViewModels;

public sealed class ShopViewModel : INotifyPropertyChanged
{
    // ─── backing storage ───────────────────────────────────────────────────────
    private readonly ObservableCollection<Amazon.Models.CartItem> _cart = new();
    private readonly ProductServiceProxy            _productService = new();

    public  ReadOnlyObservableCollection<Amazon.Models.CartItem>  Cart     { get; }
    public  ObservableCollection<Product>           Products { get; } = new();

    // ─── commands (one instance each, all MAUI Command) ───────────────────────
    public ICommand AddToCartCommand  { get; }
    public ICommand CheckoutCommand   { get; }
    public ICommand GoToMainCommand   { get; }

    // ─── constructor ──────────────────────────────────────────────────────────
    public ShopViewModel()
    {
        Debug.WriteLine($"[ShopVM] #{GetHashCode()} ctor");

        Cart = new ReadOnlyObservableCollection<Amazon.Models.CartItem>(_cart);

        AddToCartCommand = new Command<Product?>(AddToCart, p => p != null);
        CheckoutCommand  = new Command(Checkout, () => _cart.Any());
        GoToMainCommand  = new Command(async () => await GoToMainAsync());

        _cart.CollectionChanged += (_, __) => RaiseTotals();

        _ = LoadProductsAsync();          // fire & forget
    }

    // ─── calculated props for binding ─────────────────────────────────────────
    public decimal CartSubtotal { get; private set; }
    public decimal CartTax      { get; private set; }
    public decimal CartTotal    { get; private set; }

    // ─── business logic ───────────────────────────────────────────────────────
    private async Task LoadProductsAsync()
    {
        foreach (var p in await _productService.GetAllProductsAsync())
            Products.Add(p);
    }

    private void AddToCart(Product? product)
    {
        _ = ShowAddToCartAlert();
        
        Debug.WriteLine("[ShopVM] AddToCart called.");
        if (product == null || product.StockQuantity <= 0)
        {
            Debug.WriteLine("[ShopVM] AddToCart: Product is null or out of stock. Returning.");
            return;
        }
        
        Debug.WriteLine($"[ShopVM] AddToCart: Adding Product ID {product.Id}");

        var item = _cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            Debug.WriteLine("[ShopVM] AddToCart: Creating new item");
            item = new Amazon.Models.CartItem { Product = product, Quantity = 1 };
            item.PropertyChanged += CartItem_PropertyChanged;
            _cart.Add(item);
            Debug.WriteLine($"[ShopVM] AddToCart: New item added. Cart count: {_cart.Count}");
        }
        else if (item.Quantity < product.StockQuantity)
        {
            Debug.WriteLine($"[ShopVM] AddToCart: Incrementing quantity for item {item.Product.Id}");
            item.Quantity++;
            Debug.WriteLine($"[ShopVM] AddToCart: Quantity is now {item.Quantity}");
        }
        else
        {
            Debug.WriteLine("AddToCart: Quantity cannot exceed stock");
        }

        RaiseTotals();
    }

    private async Task ShowAddToCartAlert()
    {
        try 
        {
            var page = Application.Current?.Windows[0]?.Page; 
            if (page != null)
            {
                await page.DisplayAlert("Debug", "AddToCart Called", "OK");
            }
            else
            {
                Debug.WriteLine("[ShopVM] Could not get page to display alert in AddToCart.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShopVM] Error showing AddToCart alert: {ex.Message}");
        }
    }

    private void Checkout()
    {
        if (!_cart.Any()) return;

        var receipt = BuildReceipt();
        var page = Application.Current?.Windows[0]?.Page; 
        _ = page?.DisplayAlert("Receipt", receipt, "OK");

        foreach (var ci in _cart)
            ci.PropertyChanged -= CartItem_PropertyChanged;

        _cart.Clear();
        RaiseTotals();
    }

    private static async Task GoToMainAsync()
        => await Shell.Current.GoToAsync("//MainPage");

    // ─── helpers ──────────────────────────────────────────────────────────────
    private void RaiseTotals()
    {
        Debug.WriteLine("[ShopVM] RaiseTotals called.");
        
        CartSubtotal = _cart.Sum(i => i.Subtotal);
        CartTax      = CartSubtotal * 0.07m;
        CartTotal    = CartSubtotal + CartTax;

        OnPropertyChanged(nameof(CartSubtotal));
        OnPropertyChanged(nameof(CartTax));
        OnPropertyChanged(nameof(CartTotal));

        ((Command)CheckoutCommand).ChangeCanExecute();
    }

    private string BuildReceipt() =>
        $"Receipt\n\n{string.Join('\n', _cart.Select(i =>
            $"{i.Product.Name} x{i.Quantity} @ ${i.Product.Price:F2} = ${i.Subtotal:F2}"))}\n\n" +
        $"Subtotal: ${CartSubtotal:F2}\n" +
        $"Tax (7%): ${CartTax:F2}\n" +
        $"Total:    ${CartTotal:F2}";

    // ─── INotifyPropertyChanged plumbing ──────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        Debug.WriteLine($"[ShopVM] OnPropertyChanged called for: {name}");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Debug.WriteLine($"[ShopVM] CartItem_PropertyChanged called by sender: {sender}, PropertyName: {e.PropertyName}");
        if (e.PropertyName is nameof(Amazon.Models.CartItem.Quantity) or nameof(Amazon.Models.CartItem.Subtotal))
            RaiseTotals();
    }
}
