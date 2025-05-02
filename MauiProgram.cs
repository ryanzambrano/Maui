using Microsoft.Extensions.Logging;
using Amazon.Services;
using Amazon.ViewModels;
using Amazon.Views;
using System.Diagnostics;

namespace Amazon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Debug.WriteLine("[MauiProgram] CreateMauiApp started.");
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
            Debug.WriteLine("[MauiProgram] DEBUG mode detected.");
#endif

            Debug.WriteLine("[MauiProgram] Building MauiApp...");
            var app = builder.Build();
            Debug.WriteLine("[MauiProgram] MauiApp built.");
            return app;
        }
    }
}
