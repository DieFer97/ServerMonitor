using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microcharts.Maui;
using ServerMonitor.Services;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMicrocharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("FontAwesome5Solid.otf", "FontAwesome");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<AuthViewModel>();
        builder.Services.AddSingleton<SensorService>();
        builder.Services.AddSingleton<SensorViewModel>();
        builder.Services.AddSingleton<GraphicsViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<GraphicsPage>();
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddTransient<EditProfileViewModel>();
        builder.Services.AddTransient<EditProfilePage>();

        builder.Services.AddSingleton<App>();

        builder.UseMauiApp<App>(sp => sp.GetRequiredService<App>());

        return builder.Build();
    }
}