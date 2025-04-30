using Amazon.ViewModels;
using Amazon.Services;

namespace Amazon.Views
{
    public partial class InventoryManagementView : ContentPage
    {
        public InventoryManagementView()
        {
            InitializeComponent();
            BindingContext = new InventoryManagementVM();
        }
    }
}