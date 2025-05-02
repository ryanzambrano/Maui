using Amazon.ViewModels;
using Amazon.Models;
using System.Diagnostics;

namespace Amazon.Views
{
    public partial class ShoppingCartView : ContentPage
    {
        private readonly ShopViewModel _viewModel;
        
        public ShoppingCartView(ShopViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _viewModel = vm;
        }
        
        private void OnRemoveFromCartClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is Amazon.Models.CartItem cartItem)
                {
                    _viewModel.RemoveFromCart(cartItem);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnRemoveFromCartClicked: {ex.Message}");
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh the UI when navigating to this page
            if (_viewModel != null)
            {
                // The viewmodel will handle updating the UI when cart selection changes
            }
        }
        
        // Cart tab handling
        private void OnCartTabTapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (sender is Element element && 
                    element.BindingContext is Amazon.ViewModels.ShoppingCart cart && 
                    _viewModel != null)
                {
                    // Get the index of the cart in the collection
                    int index = _viewModel.ShoppingCarts.IndexOf(cart);
                    if (index >= 0)
                    {
                        // Set the selected cart index
                        _viewModel.SelectedCartIndex = index;
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
                    _viewModel != null)
                {
                    // Make sure we don't delete the main cart
                    if (cart.Name != "Shopping Cart")
                    {
                        // Use the view model to handle the deletion
                        int index = _viewModel.ShoppingCarts.IndexOf(cart);
                        if (index >= 0)
                        {
                            _viewModel.SelectedCartIndex = index; // Select the cart first
                            _viewModel.DeleteCurrentCart(); // Then delete it
                        }
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
                if (_viewModel != null)
                {
                    // Use the view model to create a new cart
                    _viewModel.CreateNewCart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAddNewCartClicked: {ex.Message}");
            }
        }
    }
} 