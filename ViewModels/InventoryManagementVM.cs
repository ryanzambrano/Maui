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

        public event PropertyChangedEventHandler? PropertyChanged;

        public InventoryManagementVM()
        {
            _products = new ObservableCollection<Product>();
            _cart = new ObservableCollection<CartItem>();
            _productService = new ProductServiceProxy();
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
            }
        }

        public ICommand AddProductCommand => new Command(AddProduct);
        public ICommand UpdateProductCommand => new Command(UpdateProduct);
        public ICommand DeleteProductCommand => new Command(DeleteProduct);
        public ICommand GoToShopCommand => new Command(GoToShop);
        public ICommand AddToCartCommand => new Command<Product?>(AddToCart);
        public ICommand RemoveFromCartCommand => new Command<CartItem?>(RemoveFromCart);
        public ICommand CheckoutCommand => new Command(Checkout);
        public ICommand GoToInventoryCommand => new Command(GoToInventory);

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

        private void GoToShop()
        {
            if (Application.Current?.Windows[0]?.Page is MainPage mainPage)
            {
                mainPage.Navigation.PushAsync(new ShopView());
            }
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

        private async void GoToInventory()
        {
            await Shell.Current.GoToAsync(nameof(InventoryManagementView));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Simple Command implementation for MAUI
        public class Command : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool>? _canExecute;

            public Command(Action execute, Func<bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

            public void Execute(object? parameter) => _execute();

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public class Command<T> : ICommand
        {
            private readonly Action<T?> _execute;
            private readonly Func<T?, bool>? _canExecute;

            public Command(Action<T?> execute, Func<T?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => 
                parameter is T t ? _canExecute?.Invoke(t) ?? true : true;

            public void Execute(object? parameter)
            {
                if (parameter is T t)
                {
                    _execute(t);
                }
            }

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Product.Price * Quantity;
        public decimal Tax => Subtotal * 0.07m;
        public decimal Total => Subtotal + Tax;
    }
}