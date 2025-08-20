using Microsoft.Extensions.DependencyInjection;
using ServerMonitor.Services;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

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
                fonts.AddFont("FontAwesome5Solid.otf", "FontAwesome");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<SensorService>();
        builder.Services.AddSingleton<SensorViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}