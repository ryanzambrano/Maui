using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class ShopView : ContentPage
    {
        public ShopView()
        {
            InitializeComponent();
            BindingContext = new ShopViewModel();
        }
    }
} 