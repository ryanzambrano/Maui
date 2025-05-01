namespace Amazon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Register routes
            Routing.RegisterRoute(nameof(Views.ShopView), typeof(Views.ShopView));
            Routing.RegisterRoute(nameof(Views.InventoryManagementView), typeof(Views.InventoryManagementView));
            Routing.RegisterRoute(nameof(Views.ShoppingCartView), typeof(Views.ShoppingCartView));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}