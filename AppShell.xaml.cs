using Amazon.Views;
using System.Diagnostics;

namespace Amazon
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            Debug.WriteLine("[AppShell] Constructor started.");
            InitializeComponent();
            
            // Register routes
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(InventoryManagementView), typeof(InventoryManagementView));
            Routing.RegisterRoute(nameof(ShopView), typeof(ShopView));
            Routing.RegisterRoute(nameof(ShoppingCartView), typeof(ShoppingCartView));
            Debug.WriteLine("[AppShell] Routes registered.");
        }
    }
}
