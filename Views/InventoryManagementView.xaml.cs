using Amazon.ViewModels;

namespace Amazon.Views
{
    public partial class InventoryManagementView : ContentPage
    {
        public InventoryManagementView(InventoryManagementVM viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
} 