using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Amazon.Models;
using Amazon.Services;

namespace Amazon.ViewModels
{
    public class ShopViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _products;
        private ShoppingCart _cart;

        public ObservableCollection<Product> Products
        {
            get => _products;
            private set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ShoppingCart Cart
        {
            get => _cart;
            private set
            {
                _cart = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public ShopViewModel()
        {
            // Initialize products from service
            Products = new ObservableCollection<Product>(ProductServiceProxy.Current.Products);
            Cart = new ShoppingCart();

            // Initialize commands
            AddToCartCommand = new Command<Product>(AddToCart);
            CheckoutCommand = new Command(Checkout);
        }

        private void AddToCart(Product product)
        {
            if (product == null || product.StockQuantity <= 0) return;

            var existingItem = Cart.Items.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem != null)
            {
                if (existingItem.Quantity < product.StockQuantity)
                {
                    existingItem.Quantity++;
                    product.StockQuantity--;
                }
            }
            else
            {
                Cart.Items.Add(new CartItem { Product = product, Quantity = 1 });
                product.StockQuantity--;
            }
        }

        private async void Checkout()
        {
            if (Cart.Items.Count == 0) return;

            // Create receipt
            var receipt = $"Receipt\n\n";
            foreach (var item in Cart.Items)
            {
                receipt += $"{item.Product.Name} x {item.Quantity} @ ${item.Product.Price:F2} = ${item.Subtotal:F2}\n";
            }
            receipt += $"\nSubtotal: ${Cart.Subtotal:F2}\n";
            receipt += $"Tax (7%): ${Cart.Tax:F2}\n";
            receipt += $"Total: ${Cart.Total:F2}";

            // Show receipt
            await Application.Current.MainPage.DisplayAlert("Thank you for your purchase!", receipt, "OK");

            // Update inventory in service
            foreach (var item in Cart.Items)
            {
                await ProductServiceProxy.Current.UpdateProductAsync(item.Product);
            }

            // Clear cart
            Cart.Items.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 