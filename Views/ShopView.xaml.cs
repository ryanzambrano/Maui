using Amazon.ViewModels;
using System.Diagnostics;

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
    }
} 