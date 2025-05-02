using Amazon.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Amazon.Models;

namespace Amazon.Views
{
    public partial class ShopView : ContentPage
    {
        public ShopView(ShopViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            Debug.WriteLine($"[ShopView] Constructor - ViewModel Instance HashCode: {viewModel?.GetHashCode()}");
        }
        
        private async void OnTestButtonClicked(object sender, EventArgs e)
        {
            // Change background color
            this.BackgroundColor = Colors.Purple;
            
            // Show alert
            await DisplayAlert("TEST", "TEST BUTTON CLICKED - VIEW IS WORKING", "OK");
            
            // Reset background color
            this.BackgroundColor = Colors.White;
            
            // Change the status text if possible
            if (BindingContext is ShopViewModel vm)
            {
                vm.ButtonPressStatus = "TEST BUTTON PRESSED";
            }
        }
        
        private async void OnAddToCartClicked(object sender, EventArgs e)
        {
            // Get the button that was clicked
            var button = sender as Button;
            
            // Get the product from the CommandParameter
            var product = button?.CommandParameter as Product;
            
            // Change background color
            this.BackgroundColor = Colors.Orange;
            
            // Show alert
            await DisplayAlert("ADD TO CART", "ADD TO CART BUTTON CLICKED!", "OK");
            
            // Change the status text if possible
            if (BindingContext is ShopViewModel vm)
            {
                vm.ButtonPressStatus = product != null 
                    ? $"Added {product.Name} to cart!" 
                    : "Added item to cart!";
                
                // If we have a product, call the original method too
                if (product != null)
                {
                    vm.AddToCartDirectly(product);
                }
            }
            
            // Reset background color
            this.BackgroundColor = Colors.White;
        }
    }
} 