using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Amazon.Models;
using Amazon.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;

namespace Amazon.ViewModels;

public sealed class ShopViewModel : INotifyPropertyChanged
{
    // ─── backing storage ───────────────────────────────────────────────────────
    private readonly ObservableCollection<Amazon.Models.CartItem> _cart = new();
    private readonly ProductServiceProxy _productService = new();
    private string _buttonPressStatus = "button not pressed";

    public ReadOnlyObservableCollection<Amazon.Models.CartItem> Cart { get; }
    public ObservableCollection<Product> Products { get; } = new();
    
    public string ButtonPressStatus 
    { 
        get => _buttonPressStatus; 
        set 
        { 
            if (_buttonPressStatus != value) 
            { 
                _buttonPressStatus = value;
                MainThread.BeginInvokeOnMainThread(() => OnPropertyChanged(nameof(ButtonPressStatus)));
            } 
        } 
    }

    // ─── commands (one instance each, all MAUI Command) ───────────────────────
    public ICommand AddToCartCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand GoToMainCommand { get; }

    // ─── constructor ──────────────────────────────────────────────────────────
    public ShopViewModel()
    {
        Debug.WriteLine($"[ShopVM] #{GetHashCode()} ctor");

        Cart = new ReadOnlyObservableCollection<Amazon.Models.CartItem>(_cart);

        // Simplified - call AddToCart directly
        AddToCartCommand = new Command<Product?>(AddToCart, p => p != null);
        
        CheckoutCommand = new Command(Checkout, () => _cart.Any());
        GoToMainCommand = new Command(async () => await GoToMainAsync());

        _cart.CollectionChanged += (_, __) => RaiseTotals();

        _ = LoadProductsAsync(); // fire & forget
    }

    // ─── calculated props for binding ─────────────────────────────────────────
    public decimal CartSubtotal { get; private set; }
    public decimal CartTax { get; private set; }
    public decimal CartTotal { get; private set; }

    // ─── business logic ───────────────────────────────────────────────────────
    private async Task LoadProductsAsync()
    {
        foreach (var p in await _productService.GetAllProductsAsync())
            Products.Add(p);
    }

    private void AddToCart(Product? product)
    {
        // Get current page for visual effects
        var page = Application.Current?.Windows[0]?.Page;
        
        // Just update the status text without visual effects
        if (page != null)
        {
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                // Update cart status text with product name if available
                ButtonPressStatus = product != null 
                    ? $"ADDING {product.Name} TO CART!" 
                    : "UPDATING CART!";
                
                // Reset the status message after a delay
                await Task.Delay(3000);
                ButtonPressStatus = "button not pressed";
            });
        }
        
        if (product == null || product.StockQuantity <= 0)
        {
            return;
        }
        
        var item = _cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            item = new Amazon.Models.CartItem { Product = product, Quantity = 1 };
            item.PropertyChanged += CartItem_PropertyChanged;
            _cart.Add(item);
        }
        else if (item.Quantity < product.StockQuantity)
        {
            item.Quantity++;
        }

        RaiseTotals();
    }

    private void Checkout()
    {
        if (!_cart.Any()) return;

        // Update button status text
        ButtonPressStatus = "Processing checkout...";

        // Create a copy of cart items to work with
        var cartItems = _cart.ToList();
        
        // Update stock quantities for each product in the cart
        foreach (var item in cartItems)
        {
            // Decrease the stock quantity by the purchased quantity
            item.Product.StockQuantity -= item.Quantity;
            
            // Find and update the product in the Products collection to reflect the change
            var productInList = Products.FirstOrDefault(p => p.Id == item.Product.Id);
            if (productInList != null)
            {
                // Update on the UI thread to ensure proper notifications
                MainThread.BeginInvokeOnMainThread(() => {
                    productInList.StockQuantity = item.Product.StockQuantity;
                    
                    // Remove and re-add the product to force UI refresh if needed
                    int index = Products.IndexOf(productInList);
                    if (index >= 0)
                    {
                        Products.RemoveAt(index);
                        Products.Insert(index, productInList);
                    }
                });
            }
            
            // Update the product in the service
            _ = _productService.UpdateProductAsync(item.Product);
        }

        var receipt = BuildReceipt();
        var page = Application.Current?.Windows[0]?.Page; 
        _ = page?.DisplayAlert("Receipt", receipt + "\n\nStock quantities have been updated.", "OK");

        foreach (var ci in _cart)
            ci.PropertyChanged -= CartItem_PropertyChanged;

        _cart.Clear();
        RaiseTotals();
        
        // Update status to indicate stock quantities have been updated
        ButtonPressStatus = "Checkout complete. Stock updated!";
        
        // Force UI refresh to show updated stock quantities
        MainThread.BeginInvokeOnMainThread(() => {
            // Trigger collection changed notification
            var temp = Products.ToList();
            Products.Clear();
            foreach (var p in temp)
            {
                Products.Add(p);
            }
        });
        
        // Reset the button press status after a delay
        MainThread.BeginInvokeOnMainThread(async () => {
            await Task.Delay(3000);
            ButtonPressStatus = "button not pressed";
        });
    }

    private static async Task GoToMainAsync()
        => await Shell.Current.GoToAsync("//MainPage");

    // ─── helpers ──────────────────────────────────────────────────────────────
    private void RaiseTotals()
    {
        CartSubtotal = _cart.Sum(i => i.Subtotal);
        CartTax = CartSubtotal * 0.07m;
        CartTotal = CartSubtotal + CartTax;

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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Amazon.Models.CartItem.Quantity) or nameof(Amazon.Models.CartItem.Subtotal))
            RaiseTotals();
    }

    // Improve the refresh products method to prevent duplicates
    private bool _isRefreshing = false;
    
    public async Task RefreshProductsAsync()
    {
        // Prevent multiple concurrent refreshes
        if (_isRefreshing) return;
        
        try
        {
            _isRefreshing = true;
            ButtonPressStatus = "Refreshing products...";
            
            // Get products from service
            var products = await _productService.GetAllProductsAsync();
            
            // Clear and update on main thread
            MainThread.BeginInvokeOnMainThread(() => {
                // Clear existing products
                Products.Clear();
                
                // Add new products from service
                foreach (var p in products)
                {
                    Products.Add(p);
                }
                
                ButtonPressStatus = $"Products refreshed ({Products.Count})";
            });
            
            // Reset the status message after a delay
            await Task.Delay(2000);
            ButtonPressStatus = "button not pressed";
        }
        catch (Exception ex)
        {
            ButtonPressStatus = "Error refreshing products";
            Debug.WriteLine($"Error refreshing products: {ex.Message}");
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // Add a public method that the view can call directly
    public void AddToCartDirectly(Product product)
    {
        if (product == null || product.StockQuantity <= 0)
        {
            return;
        }
        
        var item = _cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            item = new Amazon.Models.CartItem { Product = product, Quantity = 1 };
            item.PropertyChanged += CartItem_PropertyChanged;
            _cart.Add(item);
        }
        else if (item.Quantity < product.StockQuantity)
        {
            item.Quantity++;
        }

        RaiseTotals();
    }
}
