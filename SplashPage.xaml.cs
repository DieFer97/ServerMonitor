using Microsoft.Extensions.DependencyInjection;

namespace ServerMonitor;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(1500);

        var login = App.Services!.GetRequiredService<LoginPage>();
        Application.Current!.MainPage = new NavigationPage(login);
    }
}