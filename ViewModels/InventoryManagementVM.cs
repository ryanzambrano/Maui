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

namespace Amazon.ViewModels
{
    public class InventoryManagementVM : INotifyPropertyChanged
    {
        private Product _selectedProduct;
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

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
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

        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand CheckoutCommand { get; }

        public InventoryManagementVM()
        {
            // Initialize Products with current products from service
            Products = new ObservableCollection<Product>(ProductServiceProxy.Current.Products);
            Cart = new ShoppingCart();

            // Initialize SelectedProduct to prevent null reference
            SelectedProduct = new Product();

            // Initialize Commands
            AddProductCommand = new Command(async () => await AddProductAsync());
            UpdateProductCommand = new Command(async () => await UpdateProductAsync());
            DeleteProductCommand = new Command(async () => await DeleteProductAsync());
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
                }
            }
            else
            {
                Cart.Items.Add(new CartItem { Product = product, Quantity = 1 });
            }
        }

        private void Checkout()
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
            Application.Current.MainPage.DisplayAlert("Receipt", receipt, "OK");

            // Clear cart
            Cart.Items.Clear();
        }

        public async Task AddProductAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedProduct.Name))
            {
                return;
            }

            await ProductServiceProxy.Current.AddProductAsync(SelectedProduct);
            Products.Add(SelectedProduct);
            SelectedProduct = new Product();
        }

        public async Task UpdateProductAsync()
        {
            if (SelectedProduct == null || SelectedProduct.Id == 0)
            {
                return;
            }

            await ProductServiceProxy.Current.UpdateProductAsync(SelectedProduct);
            var index = Products.IndexOf(Products.FirstOrDefault(p => p.Id == SelectedProduct.Id));
            if (index != -1)
            {
                Products[index] = SelectedProduct;
            }
        }

        public async Task DeleteProductAsync()
        {
            if (SelectedProduct == null || SelectedProduct.Id == 0)
            {
                return;
            }

            await ProductServiceProxy.Current.DeleteProductAsync(SelectedProduct.Id);
            Products.Remove(SelectedProduct);
            SelectedProduct = new Product();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Simple Command implementation for MAUI
        public class Command : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public Command(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

            public void Execute(object parameter) => _execute();

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Product.Price * Quantity;
    }

    public class ShoppingCart
    {
        public ObservableCollection<CartItem> Items { get; } = new();
        public decimal Subtotal => Items.Sum(item => item.Subtotal);
        public decimal Tax => Subtotal * 0.07m;
        public decimal Total => Subtotal + Tax;
    }
}