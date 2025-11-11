using Microsoft.Maui.Controls;
using ServerMonitor.Services;
using ServerMonitor.Helpers;

namespace ServerMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(GraphicsPage), typeof(GraphicsPage));
        Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("register", typeof(RegisterPage));

        BindingContext = this;
    }

    public Command NavigateToMainPageCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        Shell.Current.FlyoutIsPresented = false;
    });

    public Command NavigateToGraphicsPageCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync($"//{nameof(GraphicsPage)}");
        Shell.Current.FlyoutIsPresented = false;
    });

    public Command NavigateToEditProfileCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
        Shell.Current.FlyoutIsPresented = false;
    });

    public Command CerrarSesionCommand => new Command(async () =>
    {
        await OnCerrarSesion();
    });

    private async Task OnCerrarSesion()
    {
        bool confirmar = await DisplayAlert(
            "Cerrar Sesión",
            "¿Estás seguro que deseas cerrar sesión?",
            "Sí",
            "No"
        );

        if (confirmar)
        {
            var authService = ServiceHelper.GetService<AuthService>();
            if (authService != null)
            {
                await authService.LogoutAsync();
            }

            Shell.Current.FlyoutIsPresented = false;

            Application.Current.MainPage = new NavigationPage(new LoginPage(
                ServiceHelper.GetService<ViewModels.AuthViewModel>()
            ));
        }
    }
}