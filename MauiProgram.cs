using Microsoft.Extensions.Logging;
using Amazon.Services;
using Amazon.ViewModels;
using Amazon.Views;

namespace Amazon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<ProductServiceProxy>();

            // Register ViewModels
            builder.Services.AddSingleton<ShopViewModel>();
            builder.Services.AddSingleton<InventoryManagementVM>();

            // Register Views
            builder.Services.AddTransient<ShopView>();
            builder.Services.AddTransient<InventoryManagementView>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
