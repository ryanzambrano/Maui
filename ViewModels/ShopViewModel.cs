using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Amazon.Models;
using Amazon.Services;
using Amazon.Views;

namespace Amazon.ViewModels
{
    public class ShopViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<CartItem> _cart;
        private readonly ProductServiceProxy _productService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ShopViewModel()
        {
            Debug.WriteLine($"[ShopViewModel] Constructor - Instance HashCode: {this.GetHashCode()}");
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
                OnPropertyChanged(nameof(Cart.Count));
            }
        }

        public ICommand AddToCartCommand => new Command<Product?>(AddToCart);
        public ICommand CheckoutCommand => new Command(Checkout);
        public ICommand GoToMainCommand => new Command(GoToMain);

        private async void LoadProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private void AddToCart(Product? product)
        {
            Debug.WriteLine($"[ShopViewModel] AddToCart called - Instance HashCode: {this.GetHashCode()}, Product: {product?.Name}");
            if (product == null || product.StockQuantity <= 0) return;

            var existingItem = Cart.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
            {
                if (existingItem.Quantity < product.StockQuantity)
                {
                    existingItem.Quantity++;
                }
            }
            else
            {
                var newItem = new CartItem { Product = product, Quantity = 1 };
                Cart.Add(newItem);
            }
            
            // Force UI update
            Debug.WriteLine($"[ShopViewModel] Cart Count Before Notify: {Cart.Count}");
            OnPropertyChanged(nameof(Cart));
            OnPropertyChanged(nameof(Cart.Count));
            // Explicitly notify calculated properties
            OnPropertyChanged(nameof(CartSubtotal));
            OnPropertyChanged(nameof(CartTax));
            OnPropertyChanged(nameof(CartTotal));
            Debug.WriteLine($"[ShopViewModel] PropertyChanged notifications sent for Cart.");
        }

        private void Checkout()
        {
            if (Cart.Count == 0) return;

            var receipt = $"Receipt\n\n";
            foreach (var item in Cart)
            {
                receipt += $"{item.Product.Name} x {item.Quantity} @ ${item.Product.Price:F2} = ${item.Subtotal:F2}\n";
            }
            receipt += $"\nSubtotal: ${CartSubtotal:F2}\n";
            receipt += $"Tax (7%): ${CartTax:F2}\n";
            receipt += $"Total: ${CartTotal:F2}";

            if (Application.Current?.Windows[0]?.Page is MainPage mainPage)
            {
                mainPage.DisplayAlert("Receipt", receipt, "OK");
            }

            Cart.Clear();
            OnPropertyChanged(nameof(Cart));
            OnPropertyChanged(nameof(Cart.Count));
            // Explicitly notify calculated properties
            OnPropertyChanged(nameof(CartSubtotal));
            OnPropertyChanged(nameof(CartTax));
            OnPropertyChanged(nameof(CartTotal));
        }

        private async void GoToMain()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        // Add calculated properties for binding
        public decimal CartSubtotal => Cart.Sum(item => item.Subtotal);
        public decimal CartTax => CartSubtotal * 0.07m;
        public decimal CartTotal => CartSubtotal + CartTax;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 