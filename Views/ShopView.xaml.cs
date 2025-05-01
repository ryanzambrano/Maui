using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class ShopView : ContentPage
    {
        public ShopView(ShopViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 