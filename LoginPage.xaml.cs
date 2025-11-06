using ServerMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ServerMonitor;

public partial class LoginPage : ContentPage
{
    public LoginPage(AuthViewModel authViewModel)
    {
        InitializeComponent();
        BindingContext = authViewModel;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var success = await ((AuthViewModel)BindingContext).LoginAsync();
        if (success)
        {
            try
            {
                while (Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync(false);
                }

                Application.Current.MainPage = new AppShell();

                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[v0] Navigation Error: {ex.Message}\n{ex.StackTrace}");

                Application.Current.MainPage = new AppShell();
            }
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        var page = App.Services!.GetRequiredService<RegisterPage>();
        await Navigation.PushModalAsync(new NavigationPage(page));
    }
}