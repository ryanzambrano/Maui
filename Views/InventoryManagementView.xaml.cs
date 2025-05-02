using Amazon.ViewModels;
using Microsoft.Maui.Graphics;

namespace Amazon.Views
{
    public partial class InventoryManagementView : ContentPage
    {
        private readonly InventoryManagementVM _viewModel;
        
        public InventoryManagementView(InventoryManagementVM viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }
        
        private async void OnTestButtonClicked(object sender, EventArgs e)
        {
            // Visual feedback 1: Change page background color
            this.BackgroundColor = Colors.Red;
            
            // Visual feedback 2: Show alert
            await DisplayAlert("Test", "Button Clicked!", "OK");
            
            // Visual feedback 3: Try to start editing mode
            if (BindingContext is InventoryManagementVM vm && vm.SelectedProduct != null)
            {
                // Direct call to set editing mode
                vm.IsEditing = true;
                
                // Log to debug output
                System.Diagnostics.Debug.WriteLine($"Test button clicked, IsEditing set to {vm.IsEditing}");
            }
            
            // Reset background
            this.BackgroundColor = Colors.White;
        }
        
        private void OnEditProductClicked(object sender, EventArgs e)
        {
            if (BindingContext is InventoryManagementVM vm && vm.SelectedProduct != null)
            {
                // Direct call to set editing mode
                vm.IsEditing = true;
                
                // Log to debug output
                System.Diagnostics.Debug.WriteLine($"Edit Product button clicked, IsEditing set to {vm.IsEditing}");
            }
        }
        
        private async void OnTestDoneClicked(object sender, EventArgs e)
        {
            // Visual feedback 1: Change page background color
            this.BackgroundColor = Colors.Green;
            
            // Visual feedback 2: Show alert
            await DisplayAlert("Test", "Done Button Clicked!", "OK");
            
            // Visual feedback 3: Try to exit editing mode
            if (BindingContext is InventoryManagementVM vm)
            {
                // Direct call to exit editing mode
                vm.IsEditing = false;
                
                // Log to debug output
                System.Diagnostics.Debug.WriteLine($"Test done button clicked, IsEditing set to {vm.IsEditing}");
            }
            
            // Reset background
            this.BackgroundColor = Colors.White;
        }
        
        private void OnDoneEditingClicked(object sender, EventArgs e)
        {
            if (BindingContext is InventoryManagementVM vm)
            {
                // Save the changes
                if (vm.SelectedProduct != null)
                {
                    vm.UpdateSelectedProduct();
                }
                
                // Direct call to exit editing mode
                vm.IsEditing = false;
                
                // Log to debug output
                System.Diagnostics.Debug.WriteLine($"Done Editing button clicked, IsEditing set to {vm.IsEditing}");
            }
        }
    }
} 