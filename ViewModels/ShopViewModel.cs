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
using System.Linq;
using System.Collections.Generic;

namespace Amazon.ViewModels;

public sealed class ShopViewModel : INotifyPropertyChanged
{
    // ─── backing storage for multiple carts ─────────────────────────────────────
    private ObservableCollection<ShoppingCart> _shoppingCarts = new();
    private readonly ProductServiceProxy _productService = new();
    private string _buttonPressStatus = "button not pressed";
    private ShoppingCart? _currentCart;

    // Collection of all shopping carts/wishlists
    public ObservableCollection<ShoppingCart> ShoppingCarts => _shoppingCarts;
    
    // The current cart items (read-only view of current cart's items)
    public ReadOnlyObservableCollection<Amazon.Models.CartItem>? Cart => _currentCart?.Items;
    
    // Products from inventory
    public ObservableCollection<Product> Products { get; } = new();
    
    // Current cart name
    public string CurrentCartName => _currentCart?.Name ?? "No Cart Selected";
    
    // Current cart index for UI binding
    private int _selectedCartIndex = 0;
    public int SelectedCartIndex
    {
        get => _selectedCartIndex;
        set
        {
            if (_selectedCartIndex != value && value >= 0 && value < _shoppingCarts.Count)
            {
                _selectedCartIndex = value;
                SetCurrentCart(_shoppingCarts[value]);
                OnPropertyChanged();
            }
        }
    }
    
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

    // Sorting functionality
    private string _sortOrder = "Default";
    public string SortOrder
    {
        get => _sortOrder;
        set
        {
            if (_sortOrder != value)
            {
                _sortOrder = value;
                OnPropertyChanged();
                SortProducts(_sortOrder);
            }
        }
    }
    
    public ICommand SortByNameCommand { get; }
    public ICommand SortByPriceCommand { get; }

    // Cart sorting functionality - simplified approach
    private string _cartSortOrder = "Default";
    
    public ICommand SortCartByNameCommand { get; }
    public ICommand SortCartByPriceCommand { get; }
    
    // Commands for cart/wishlist management
    public ICommand CreateNewCartCommand { get; }
    public ICommand DeleteCurrentCartCommand { get; }

    // ─── commands (one instance each, all MAUI Command) ───────────────────────
    public ICommand AddToCartCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand GoToMainCommand { get; }

    // ─── constructor ──────────────────────────────────────────────────────────
    public ShopViewModel()
    {
        Debug.WriteLine($"[ShopVM] #{GetHashCode()} ctor");

        // Create default shopping cart
        var defaultCart = new ShoppingCart("Shopping Cart");
        _shoppingCarts.Add(defaultCart);
        
        // Create wishlist
        var wishlist = new ShoppingCart("Wishlist");
        _shoppingCarts.Add(wishlist);
        
        // Set current cart to default
        SetCurrentCart(defaultCart);

        // Simplified - call AddToCart directly
        AddToCartCommand = new Command<Product?>(AddToCart, p => p != null);
        
        // Add sort commands for products
        SortByNameCommand = new Command(() => SortProducts("Name"));
        SortByPriceCommand = new Command(() => SortProducts("Price"));
        
        // Add simple sort commands for cart items
        SortCartByNameCommand = new Command(() => SortCartItems("Name"));
        SortCartByPriceCommand = new Command(() => SortCartItems("Price"));
        
        // Add cart management commands
        CreateNewCartCommand = new Command(CreateNewCart);
        DeleteCurrentCartCommand = new Command(DeleteCurrentCart, () => _shoppingCarts.Count > 1);
        
        CheckoutCommand = new Command(Checkout, () => _currentCart?.Items.Count > 0);
        GoToMainCommand = new Command(async () => await GoToMainAsync());

        _ = LoadProductsAsync(); // fire & forget
    }
    
    // Set the current active cart and update all relevant properties
    private void SetCurrentCart(ShoppingCart cart)
    {
        if (_currentCart != null)
        {
            // Unsubscribe from old cart events - using the cart's ItemsChanged event instead
            _currentCart.ItemsChanged -= OnCartItemsChanged;
        }
        
        _currentCart = cart;
        
        // Subscribe to new cart's ItemsChanged event
        if (_currentCart != null)
        {
            _currentCart.ItemsChanged += OnCartItemsChanged;
        }
        
        // Update UI properties
        OnPropertyChanged(nameof(Cart));
        OnPropertyChanged(nameof(CurrentCartName));
        
        // Update totals for the new cart
        RaiseTotals();
    }
    
    // Handle cart item changes
    private void OnCartItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaiseTotals();
    }
    
    // Create a new shopping cart/wishlist
    public void CreateNewCart()
    {
        // Get name from user
        MainThread.BeginInvokeOnMainThread(async () => {
            // Use Windows[0].Page instead of Application.Current?.MainPage
            var mainPage = Application.Current?.Windows.Count > 0 ? 
                Application.Current.Windows[0].Page : null;
                
            string? result = await mainPage?.DisplayPromptAsync(
                "New Wishlist", 
                "Enter a name for your new wishlist:",
                "Create",
                "Cancel",
                "My Wishlist");
                
            if (!string.IsNullOrWhiteSpace(result))
            {
                var newCart = new ShoppingCart(result);
                _shoppingCarts.Add(newCart);
                
                // Switch to the new cart
                SelectedCartIndex = _shoppingCarts.Count - 1;
                
                // Update command can execute
                ((Command)DeleteCurrentCartCommand).ChangeCanExecute();
            }
        });
    }
    
    // Delete the current cart
    public void DeleteCurrentCart()
    {
        try
        {
            // Don't allow deletion of the main "Shopping Cart"
            if (_currentCart?.Name == "Shopping Cart")
            {
                MainThread.BeginInvokeOnMainThread(() => {
                    ButtonPressStatus = "Cannot delete the main Shopping Cart";
                    
                    // Reset the message after a delay
                    Task.Run(async () => {
                        await Task.Delay(2000);
                        ButtonPressStatus = "button not pressed";
                    });
                });
                return;
            }
            
            if (_currentCart != null && _shoppingCarts.Count > 1)
            {
                // Capture the current cart in a local variable to avoid potential null reference
                var cartToDelete = _currentCart;
                
                MainThread.BeginInvokeOnMainThread(async () => {
                    // Use Windows[0].Page instead of Application.Current?.MainPage
                    var mainPage = Application.Current?.Windows.Count > 0 ? 
                        Application.Current.Windows[0].Page : null;
                        
                    bool confirm = false;
                    if (mainPage != null && cartToDelete != null) // Double-check cartToDelete isn't null
                    {
                        confirm = await mainPage.DisplayAlert(
                            "Delete Wishlist", 
                            $"Are you sure you want to delete \"{cartToDelete.Name}\"?",
                            "Delete",
                            "Cancel");
                    }
                        
                    if (confirm && cartToDelete != null) // Check again before accessing
                    {
                        int currentIndex = _shoppingCarts.IndexOf(cartToDelete);
                        _shoppingCarts.Remove(cartToDelete);
                        
                        // Select a different cart
                        SelectedCartIndex = Math.Min(currentIndex, _shoppingCarts.Count - 1);
                        
                        // Update command can execute
                        ((Command)DeleteCurrentCartCommand).ChangeCanExecute();
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting cart: {ex.Message}");
            ButtonPressStatus = "Error deleting cart";
        }
    }

    // ─── calculated props for binding ─────────────────────────────────────────
    public decimal CartSubtotal { get; private set; }
    public decimal CartTax { get; private set; }
    public decimal CartTotal { get; private set; }

    // ─── business logic ───────────────────────────────────────────────────────
    private async Task LoadProductsAsync()
    {
        try
        {
            Debug.WriteLine("Loading products...");
            var products = await _productService.GetAllProductsAsync();
            
            MainThread.BeginInvokeOnMainThread(() => {
                Products.Clear(); // Clear first to avoid duplicates
                
                // Log how many products we're loading
                Debug.WriteLine($"Loading {products.Count()} products from service");
                
                foreach (var p in products)
                {
                    Products.Add(p);
                }
                
                Debug.WriteLine($"Loaded {Products.Count} products");
                
                // Notify the UI that products have changed
                OnPropertyChanged(nameof(Products));
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading products: {ex.Message}");
            
            // Try to populate with some sample products to avoid empty UI
            MainThread.BeginInvokeOnMainThread(() => {
                Products.Clear();
                
                // Add some sample products
                Products.Add(new Product { 
                    Id = 1, 
                    Name = "Sample Product 1", 
                    Description = "This is a sample product", 
                    Price = 19.99m, 
                    StockQuantity = 10, 
                    Category = "Sample",
                    ImageUrl = "https://example.com/sample1.jpg"
                });
                
                Products.Add(new Product { 
                    Id = 2, 
                    Name = "Sample Product 2", 
                    Description = "This is another sample product", 
                    Price = 29.99m, 
                    StockQuantity = 5, 
                    Category = "Sample",
                    ImageUrl = "https://example.com/sample2.jpg"
                });
                
                Debug.WriteLine("Added sample products");
                OnPropertyChanged(nameof(Products));
            });
        }
    }

    private void AddToCart(Product? product)
    {
        if (_currentCart == null) return;
        
        // Get current page for visual effects
        var page = Application.Current?.Windows[0]?.Page;
        
        // Capture the current cart to avoid issues in the async lambda
        var targetCart = _currentCart;
        
        // Just update the status text without visual effects
        if (page != null)
        {
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                // Update cart status text with product name if available
                if (targetCart != null) // Make sure we still have a valid cart
                {
                    ButtonPressStatus = product != null 
                        ? $"ADDING {product.Name} TO {targetCart.Name}!" 
                        : "UPDATING CART!";
                }
                else
                {
                    ButtonPressStatus = "Unable to update cart!";
                }
                
                // Reset the status message after a delay
                await Task.Delay(3000);
                ButtonPressStatus = "button not pressed";
            });
        }
        
        if (product == null || product.StockQuantity <= 0)
        {
            return;
        }
        
        var cart = _currentCart.Items;
        var item = cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            item = new Amazon.Models.CartItem { Product = product, Quantity = 1 };
            _currentCart.AddItem(item);
        }
        else if (item.Quantity < product.StockQuantity)
        {
            item.Quantity++;
        }

        RaiseTotals();
    }

    private void Checkout()
    {
        try
        {
            if (_currentCart == null || _currentCart.Items.Count == 0) return;

            // Update button status text
            ButtonPressStatus = "Processing checkout...";

            // Create a copy of cart items to work with
            var cartItems = _currentCart.Items.ToList();
            
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
            _ = page?.DisplayAlert("Receipt", receipt, "OK");

            // Clear the current cart
            _currentCart.Clear();
            
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
        catch (Exception ex)
        {
            Debug.WriteLine($"Checkout error: {ex.Message}");
            ButtonPressStatus = "Checkout failed!";
        }
    }

    private static async Task GoToMainAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    // ─── helpers ──────────────────────────────────────────────────────────────
    private void RaiseTotals()
    {
        if (_currentCart == null)
        {
            CartSubtotal = 0;
            CartTax = 0;
            CartTotal = 0;
        }
        else
        {
            CartSubtotal = _currentCart.Items.Sum(i => i.Subtotal);
            
            // Get the configured tax rate (default to 7% if not configured)
            decimal taxRate = Preferences.ContainsKey("TaxRate") 
                ? (decimal)Preferences.Get("TaxRate", 0.07) 
                : 0.07m;
                
            CartTax = CartSubtotal * taxRate;
            CartTotal = CartSubtotal + CartTax;
        }

        OnPropertyChanged(nameof(CartSubtotal));
        OnPropertyChanged(nameof(CartTax));
        OnPropertyChanged(nameof(CartTotal));
        
        if (CheckoutCommand is Command cmd)
        {
            cmd.ChangeCanExecute();
        }
    }

    private string BuildReceipt()
    {
        if (_currentCart == null) return "No cart selected";
        
        // Get the configured tax rate (as a percentage for display)
        decimal taxRate = Preferences.ContainsKey("TaxRate") 
            ? (decimal)Preferences.Get("TaxRate", 0.07) 
            : 0.07m;
        
        string taxRateDisplay = $"{taxRate * 100:F1}%";
        
        return $"Receipt for {_currentCart.Name}\n\n{string.Join('\n', _currentCart.Items.Select(i =>
            $"{i.Product.Name} x{i.Quantity} @ ${i.Product.Price:F2} = ${i.Subtotal:F2}"))}\n\n" +
        $"Subtotal: ${CartSubtotal:F2}\n" +
        $"Tax ({taxRateDisplay}): ${CartTax:F2}\n" +
        $"Total:    ${CartTotal:F2}";
    }

    // ─── INotifyPropertyChanged plumbing ──────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        if (_currentCart == null) return;
        
        if (product == null || product.StockQuantity <= 0)
        {
            return;
        }
        
        var cart = _currentCart.Items;
        var item = cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            item = new Amazon.Models.CartItem { Product = product, Quantity = 1 };
            _currentCart.AddItem(item);
        }
        else if (item.Quantity < product.StockQuantity)
        {
            item.Quantity++;
        }

        RaiseTotals();
    }

    // Method to remove an item from the cart
    public void RemoveFromCart(Amazon.Models.CartItem item)
    {
        if (_currentCart == null) return;
        
        // Capture cart reference for async lambda
        var targetCart = _currentCart;
        
        if (item != null)
        {
            // Remove the item from the cart
            _currentCart.RemoveItem(item);
            
            // Update cart totals
            RaiseTotals();
            
            // Update status
            MainThread.BeginInvokeOnMainThread(async () => {
                if (targetCart != null) // Verify cart still exists
                {
                    ButtonPressStatus = $"Removed {item.Product.Name} from {targetCart.Name}";
                    await Task.Delay(2000);
                    ButtonPressStatus = "button not pressed";
                }
            });
        }
    }

    // Method to remove a cart item by reference (without explicit type casting)
    public void RemoveCartItemByReference(object cartItemObj)
    {
        if (_currentCart == null) return;
        
        // Capture cart reference for async lambda
        var targetCart = _currentCart;
        
        // Find the cart item in our collection that matches the reference
        var itemToRemove = _currentCart.Items.FirstOrDefault(item => item == cartItemObj);
        
        if (itemToRemove != null)
        {
            // Remove from cart
            _currentCart.RemoveItem(itemToRemove);
            
            // Update cart totals
            RaiseTotals();
            
            // Show feedback
            MainThread.BeginInvokeOnMainThread(async () => {
                if (targetCart != null) // Verify cart still exists
                {
                    ButtonPressStatus = $"Removed {itemToRemove.Product.Name} from {targetCart.Name}";
                    await Task.Delay(2000);
                    ButtonPressStatus = "button not pressed";
                }
            });
        }
    }

    // Method to add multiple items to cart at once
    public void AddMultipleToCart(Product product, int quantity)
    {
        if (_currentCart == null) return;
        
        // Capture cart reference for async lambda
        var targetCart = _currentCart;
        
        if (product == null || product.StockQuantity <= 0 || quantity <= 0)
        {
            return;
        }

        // Limit quantity to available stock
        quantity = Math.Min(quantity, product.StockQuantity);
        
        var cart = _currentCart.Items;
        var item = cart.FirstOrDefault(ci => ci.Product.Id == product.Id);
        if (item is null)
        {
            item = new Amazon.Models.CartItem { Product = product, Quantity = quantity };
            _currentCart.AddItem(item);
        }
        else
        {
            // Update quantity (limited by stock)
            int newQuantity = Math.Min(item.Quantity + quantity, product.StockQuantity);
            item.Quantity = newQuantity;
        }

        RaiseTotals();
        
        // Store the quantity for the lambda
        int addedQuantity = quantity;
        
        // Update status to confirm items added
        MainThread.BeginInvokeOnMainThread(async () => {
            if (targetCart != null) // Verify cart still exists
            {
                ButtonPressStatus = $"Added {addedQuantity} {product.Name} to {targetCart.Name}";
                await Task.Delay(2000);
                ButtonPressStatus = "button not pressed";
            }
        });
    }

    private void SortProducts(string sortOrder)
    {
        if (Products == null || Products.Count == 0) return;
        
        var sorted = sortOrder switch
        {
            "Name" => Products.OrderBy(p => p.Name).ToList(),
            "Price" => Products.OrderBy(p => p.Price).ToList(),
            _ => Products.ToList() // Default or unknown sort order
        };
        
        // Update collection
        Products.Clear();
        foreach (var product in sorted)
        {
            Products.Add(product);
        }
    }

    // Simple method to sort cart items
    private void SortCartItems(string sortOrder)
    {
        if (_currentCart == null || _currentCart.Items.Count == 0) return;
        
        _cartSortOrder = sortOrder;
        _currentCart.SortItems(sortOrder);
    }
}

// Class to represent a shopping cart or wishlist
public class ShoppingCart
{
    private readonly ObservableCollection<Amazon.Models.CartItem> _items = new();
    
    public ShoppingCart(string name)
    {
        Name = name;
        Items = new ReadOnlyObservableCollection<Amazon.Models.CartItem>(_items);
        
        // Subscribe to collection changes internally
        _items.CollectionChanged += (s, e) => ItemsChanged?.Invoke(s, e);
    }
    
    public string Name { get; }
    
    public ReadOnlyObservableCollection<Amazon.Models.CartItem> Items { get; }
    
    // Event that will be raised when the underlying collection changes
    public event System.Collections.Specialized.NotifyCollectionChangedEventHandler? ItemsChanged;
    
    public void AddItem(Amazon.Models.CartItem item)
    {
        // Subscribe to property changes on the item
        item.PropertyChanged += Item_PropertyChanged;
        _items.Add(item);
    }
    
    public void RemoveItem(Amazon.Models.CartItem item)
    {
        // Unsubscribe from property changes
        item.PropertyChanged -= Item_PropertyChanged;
        _items.Remove(item);
    }
    
    public void Clear()
    {
        // Unsubscribe from all items
        foreach (var item in _items)
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }
        
        _items.Clear();
    }
    
    public void SortItems(string sortOrder)
    {
        // Sort items by name or price
        var sorted = sortOrder switch
        {
            "Name" => _items.OrderBy(item => item.Product.Name).ToList(),
            "Price" => _items.OrderBy(item => item.Product.Price).ToList(),
            _ => _items.ToList() // Default
        };
        
        // Clear and rebuild the cart collection
        var tempItems = new List<Amazon.Models.CartItem>(sorted);
        _items.Clear();
        
        foreach (var item in tempItems)
        {
            _items.Add(item);
        }
    }
    
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Forward property change events to ensure UI updates
        // This is needed for quantity changes
    }
}
