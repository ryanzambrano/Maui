using Amazon.Views;
using Amazon.ViewModels;
using Microsoft.Maui.Controls;

namespace Amazon
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainVM(); // Set the BindingContext to the current page
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Views.InventoryManagementView());
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Views.ShopView());
        }
    }
}