using Amazon.Views;
using Amazon.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace Amazon
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            Debug.WriteLine("[MainPage] Constructor started.");
            InitializeComponent();
            BindingContext = new MainVM(); // Set the BindingContext to the current page
            Debug.WriteLine("[MainPage] Constructor finished.");
        }

        private async void OnInventoryClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[MainPage] OnInventoryClicked.");
            await Shell.Current.GoToAsync("//InventoryManagementView");
        }

        private async void OnShopClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[MainPage] OnShopClicked.");
            await Shell.Current.GoToAsync("//ShopView");
        }
        
        private async void OnConfigClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("[MainPage] OnConfigClicked.");
            await Shell.Current.GoToAsync("//ConfigView");
        }
    }
}