using ServerMonitor.ViewModels;

namespace ServerMonitor;

public partial class EditProfilePage : ContentPage
{
    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((EditProfileViewModel)BindingContext).LoadCurrentUserAsync();
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        var success = await ((EditProfileViewModel)BindingContext).UpdateProfileAsync();
        if (success)
        {
            await DisplayAlert("Éxito", "Perfil actualizado correctamente", "OK");
        }
    }
}