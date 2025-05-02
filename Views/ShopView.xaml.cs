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
            
            // Always refresh products - we need to make sure this works
            Debug.WriteLine("[ShopView] OnAppearing - Refreshing products");
            RefreshProducts();
        }
        
        private void RefreshProducts()
        {
            try
            {
                // Make sure we have a ViewModel
                if (_viewModel == null)
                {
                    Debug.WriteLine("[ShopView] RefreshProducts - ViewModel is null");
                    return;
                }
                
                Debug.WriteLine("[ShopView] RefreshProducts - Starting refresh");
                
                // Call RefreshProductsAsync directly on the UI thread
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    try 
                    {
                        // Show a loading indicator 
                        _viewModel.ButtonPressStatus = "Loading products...";
                        
                        // Refresh products
                        await _viewModel.RefreshProductsAsync();
                        
                        // Report the number of products loaded
                        Debug.WriteLine($"[ShopView] RefreshProducts - Loaded {_viewModel.Products.Count} products");
                        
                        if (_viewModel.Products.Count == 0)
                        {
                            // If no products were loaded, try accessing the service directly
                            Debug.WriteLine("[ShopView] RefreshProducts - No products loaded, trying direct access");
                            
                            var service = new Amazon.Services.ProductServiceProxy();
                            var products = await service.GetAllProductsAsync();
                            
                            Debug.WriteLine($"[ShopView] RefreshProducts - Direct access found {products.Count} products");
                            
                            // Add the products directly
                            foreach (var product in products)
                            {
                                _viewModel.Products.Add(product);
                            }
                            
                            Debug.WriteLine($"[ShopView] RefreshProducts - Products count now: {_viewModel.Products.Count}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ShopView] Error refreshing products: {ex.Message}");
                        _viewModel.ButtonPressStatus = "Error loading products";
                    }
                    finally
                    {
                        // Clear the status after a delay
                        await Task.Delay(1000);
                        _viewModel.ButtonPressStatus = "button not pressed";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ShopView] Error in RefreshProducts: {ex.Message}");
            }
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
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAddToCartClicked: {ex.Message}");
            }
        }
        
        private void OnRemoveCartItemClicked(object sender, EventArgs e)
        {
            try
            {
                if (BindingContext is ShopViewModel vm)
                {
                    // Get the button and its CommandParameter
                    var button = sender as Button;
                    var cartItem = button?.CommandParameter;
                    
                    // Let the ViewModel handle the type conversion - it knows which CartItem to use
                    if (cartItem != null)
                    {
                        vm.RemoveCartItemByReference(cartItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnRemoveCartItemClicked: {ex.Message}");
            }
        }
        
        private void OnAddMultipleToCartClicked(object sender, EventArgs e)
        {
            try
            {
                // Find the button that was clicked
                var button = sender as Button;
                
                // Find the parent grid
                if (button?.Parent is HorizontalStackLayout stack && 
                    stack.Children.FirstOrDefault(c => c is Entry) is Entry quantityEntry && 
                    button.CommandParameter is Product product)
                {
                    // Try to parse the quantity
                    if (int.TryParse(quantityEntry.Text, out int quantity) && quantity > 0)
                    {
                        if (BindingContext is ShopViewModel vm)
                        {
                            // Add multiple items to cart
                            vm.AddMultipleToCart(product, quantity);
                        }
                    }
                    else
                    {
                        // Show error if the quantity is invalid
                        DisplayAlert("Invalid Quantity", "Please enter a valid quantity greater than 0.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAddMultipleToCartClicked: {ex.Message}");
            }
        }

        // Cart tab handling

        private void OnCartTabTapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (sender is Element element && 
                    element.BindingContext is Amazon.ViewModels.ShoppingCart cart && 
                    BindingContext is ShopViewModel vm)
                {
                    // Get the index of the cart in the collection
                    int index = vm.ShoppingCarts.IndexOf(cart);
                    if (index >= 0)
                    {
                        // Set the selected cart index
                        vm.SelectedCartIndex = index;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnCartTabTapped: {ex.Message}");
            }
        }

        private void OnDeleteCartClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && 
                    button.CommandParameter is Amazon.ViewModels.ShoppingCart cart && 
                    BindingContext is ShopViewModel vm)
                {
                    // Get the index of the cart in the collection
                    int index = vm.ShoppingCarts.IndexOf(cart);
                    if (index >= 0)
                    {
                        vm.SelectedCartIndex = index; // Select the cart first
                        vm.DeleteCurrentCart(); // Then delete it
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDeleteCartClicked: {ex.Message}");
            }
        }

        private void OnAddNewCartClicked(object sender, EventArgs e)
        {
            try
            {
                if (BindingContext is ShopViewModel vm)
                {
                    // Use the view model to create a new cart
                    vm.CreateNewCart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAddNewCartClicked: {ex.Message}");
            }
        }
    }
} 