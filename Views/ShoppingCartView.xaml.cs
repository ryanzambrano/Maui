using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class ShoppingCartView : ContentPage
    {
        public ShoppingCartView()
        {
            InitializeComponent();
            BindingContext = new InventoryManagementVM();
        }
    }
} 