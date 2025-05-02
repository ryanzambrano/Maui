using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class ShoppingCartView : ContentPage
{
    public ShoppingCartView(InventoryManagementVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
} 