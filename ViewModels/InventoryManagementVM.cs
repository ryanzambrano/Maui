using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Models;
using Amazon.Services;
using Microsoft.Maui.Controls;
using Amazon.Views;

namespace Amazon.ViewModels
{
    public class InventoryManagementVM : INotifyPropertyChanged
    {
        private Product? _selectedProduct;
        private ObservableCollection<Product> _products;
        private ObservableCollection<CartItem> _cart;
        private readonly ProductServiceProxy _productService;
        private bool _isEditing = false;

        // ─── commands (all set in constructor) ─────────────────────────────────────
        private Command _startEditingCommand;
        private Command _finishEditingCommand;
        
        public ICommand StartEditingCommand => _startEditingCommand;
        public ICommand FinishEditingCommand => _finishEditingCommand;
        
        public ICommand AddProductCommand => new Command(AddProduct);
        public ICommand UpdateProductCommand => new Command(UpdateProduct);
        public ICommand DeleteProductCommand => new Command(DeleteProduct);
        public ICommand GoToMainCommand => new Command(GoToMain);
        public ICommand GoToShopCommand => new Command(GoToShop);
        public ICommand AddToCartCommand => new Command<Product?>(AddToCart);
        public ICommand RemoveFromCartCommand => new Command<CartItem?>(RemoveFromCart);
        public ICommand CheckoutCommand => new Command(Checkout);
        public ICommand GoToInventoryCommand => new Command(GoToInventory);

        public event PropertyChangedEventHandler? PropertyChanged;

        public InventoryManagementVM()
        {
            _products = new ObservableCollection<Product>();
            _cart = new ObservableCollection<CartItem>();
            _productService = new ProductServiceProxy();
            
            // Initialize commands
            _startEditingCommand = new Command(StartEditing, () => SelectedProduct != null && !IsEditing);
            _finishEditingCommand = new Command(FinishEditing, () => IsEditing);
            
            LoadProducts();
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CartItem> Cart
        {
            get => _cart;
            set
            {
                _cart = value;
                OnPropertyChanged();
            }
        }

        public Product? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                
                // When selection changes, exit edit mode
                if (value != null)
                {
                    IsEditing = false;
                }
            }
        }
        
        public bool IsEditing
        {
            get
            {
                System.Diagnostics.Debug.WriteLine($"IsEditing getter: {_isEditing}");
                return _isEditing;
            }
            set
            {
                System.Diagnostics.Debug.WriteLine($"IsEditing setter: changing from {_isEditing} to {value}");
                if (_isEditing != value)
                {
                    _isEditing = value;
                    System.Diagnostics.Debug.WriteLine($"IsEditing changed to {_isEditing}");
                    OnPropertyChanged();
                    
                    // Update command states when editing mode changes
                    (_startEditingCommand as Command)?.ChangeCanExecute();
                    (_finishEditingCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private async void LoadProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private void AddProduct()
        {
            var newProduct = new Product
            {
                Name = "New Product",
                Description = "Description",
                Price = 0,
                StockQuantity = 0,
                Category = "Category",
                ImageUrl = "https://example.com/image.jpg"
            };
            _productService.AddProduct(newProduct);
            Products.Add(newProduct);
        }

        private void UpdateProduct()
        {
            if (SelectedProduct != null)
            {
                _productService.UpdateProduct(SelectedProduct);
                LoadProducts();
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct != null)
            {
                _productService.DeleteProduct(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
            }
        }

        private async void GoToShop()
        {
            await Shell.Current.GoToAsync("//ShopView");
        }

        private void AddToCart(Product? product)
        {
            if (product == null) return;
            var existingItem = Cart.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Cart.Add(new CartItem { Product = product, Quantity = 1 });
            }
        }

        private void RemoveFromCart(CartItem? item)
        {
            if (item == null) return;
            Cart.Remove(item);
        }

        private void Checkout()
        {
            if (Cart.Count == 0) return;

            var receipt = $"Receipt\n\n";
            foreach (var item in Cart)
            {
                receipt += $"{item.Product.Name} x {item.Quantity} @ ${item.Product.Price:F2} = ${item.Subtotal:F2}\n";
            }
            receipt += $"\nSubtotal: ${Cart.Sum(item => item.Subtotal):F2}\n";
            receipt += $"Tax (7%): ${Cart.Sum(item => item.Tax):F2}\n";
            receipt += $"Total: ${Cart.Sum(item => item.Total):F2}";

            if (Application.Current?.Windows[0]?.Page is MainPage mainPage)
            {
                mainPage.DisplayAlert("Receipt", receipt, "OK");
            }

            Cart.Clear();
        }

        private async void GoToMain()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void GoToInventory()
        {
            await Shell.Current.GoToAsync("//InventoryManagementView");
        }
        
        private void StartEditing()
        {
            // Make sure this method is actually being called
            System.Diagnostics.Debug.WriteLine($"StartEditing called - SelectedProduct: {SelectedProduct?.Name}");
            
            // Directly set the field to ensure it changes
            _isEditing = true;
            OnPropertyChanged(nameof(IsEditing));
            
            // Update command states when editing mode changes
            (_startEditingCommand as Command)?.ChangeCanExecute();
            (_finishEditingCommand as Command)?.ChangeCanExecute();
            
            // Confirm editing mode changed
            System.Diagnostics.Debug.WriteLine($"IsEditing set to: {IsEditing}");
        }

        private void FinishEditing()
        {
            System.Diagnostics.Debug.WriteLine("FinishEditing called");
            
            if (SelectedProduct != null)
            {
                // Save the changes
                _productService.UpdateProduct(SelectedProduct);
            }
            
            // Directly set the field to ensure it changes
            _isEditing = false;
            OnPropertyChanged(nameof(IsEditing));
            
            // Update command states
            (_startEditingCommand as Command)?.ChangeCanExecute();
            (_finishEditingCommand as Command)?.ChangeCanExecute();
            
            System.Diagnostics.Debug.WriteLine($"IsEditing set to: {IsEditing}");
        }

        // Public method to allow code-behind to update product
        public void UpdateSelectedProduct()
        {
            if (SelectedProduct != null)
            {
                _productService.UpdateProduct(SelectedProduct);
                System.Diagnostics.Debug.WriteLine($"Updated product: {SelectedProduct.Name}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CartItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Subtotal => Product.Price * Quantity;
        public decimal Tax => Subtotal * 0.07m;
        public decimal Total => Subtotal + Tax;
    }
}