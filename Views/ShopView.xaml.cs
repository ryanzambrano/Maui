using Amazon.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Amazon.Models;

namespace Amazon.Views
{
    public partial class ShopView : ContentPage
    {
        private readonly ShopViewModel _viewModel;
        
        public ShopView(ShopViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
            Debug.WriteLine($"[ShopView] Constructor - ViewModel Instance HashCode: {viewModel?.GetHashCode()}");
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh products only if we've been navigated to (not just resumed)
            if (Shell.Current.CurrentPage == this)
            {
                Debug.WriteLine("[ShopView] OnAppearing - Refreshing products");
                RefreshProducts();
            }
        }
        
        private void RefreshProducts()
        {
            // Use Task.Run to avoid blocking the UI thread
            Task.Run(async () => 
            {
                try 
                {
                    if (_viewModel != null)
                    {
                        await _viewModel.RefreshProductsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ShopView] Error refreshing products: {ex.Message}");
                }
            });
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
        
        private void OnAddToCartClicked(object sender, EventArgs e)
        {
            // Get the button that was clicked
            var button = sender as Button;
            
            // Get the product from the CommandParameter
            var product = button?.CommandParameter as Product;
            
            // Just update the status and call the method
            if (BindingContext is ShopViewModel vm && product != null)
            {
                vm.AddToCartDirectly(product);
            }
        }
    }
} 