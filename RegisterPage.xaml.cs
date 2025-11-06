using ServerMonitor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ServerMonitor;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(AuthViewModel authViewModel)
    {
        InitializeComponent();
        BindingContext = authViewModel;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        btn.IsEnabled = false;
        btn.BackgroundColor = Colors.Gray;

        var vm = (AuthViewModel)BindingContext;
        var ok = await vm.RegisterAsync();

        btn.IsEnabled = true;
        btn.BackgroundColor = Color.FromHex("#4d62da");

        if (ok)
        {
            string nombreUsuario = vm.Nombre;
            
            await DisplayAlert("✅ Registro Exitoso", 
                $"Usuario '{nombreUsuario}' registrado exitosamente.\n\nAhora puedes iniciar sesión.", 
                "Aceptar");
            
            var loginPage = App.Services!.GetRequiredService<LoginPage>();
            await Navigation.PushModalAsync(new NavigationPage(loginPage));
        }
    }

    private async void OnLoginTapped(object sender, TappedEventArgs e)
    {
        var page = App.Services!.GetRequiredService<LoginPage>();
        await Navigation.PushModalAsync(new NavigationPage(page));
    }
}