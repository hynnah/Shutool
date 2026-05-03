using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Shutool.Services;
using Shutool.Views.Auth;
using Shutool.Views.Driver;
using Shutool.Views.Rider;
using Shutool.Views.Shared;

namespace Shutool;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("PressStart2P-Regular.ttf", "PressStart2P");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<SupabaseService>();

        // Views - Auth
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<SplashPage>();

        // Views - Rider
        builder.Services.AddTransient<RiderMainPage>();

        // Views - Driver
        builder.Services.AddTransient<DriverMainPage>();
        
        builder.Services.AddTransient<ProfilePage>();
        //builder.Services.AddTransient<RiderHistoryPage>(); // remove this line for now

        return builder.Build();
    }
}