using Microsoft.Extensions.DependencyInjection;
using ServerMonitor.ViewModels;
using ServerMonitor.Services;

namespace ServerMonitor;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }

    public App()
    {
        InitializeComponent();

        var services = new ServiceCollection();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<AuthViewModel>();
        services.AddTransient<LoginPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<MainPage>();
        services.AddTransient<GraphicsPage>();

        Services = services.BuildServiceProvider();

        MainPage = new SplashPage();
    }
}