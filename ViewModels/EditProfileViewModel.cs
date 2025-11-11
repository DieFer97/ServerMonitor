using System.ComponentModel;
using System.Runtime.CompilerServices;
using ServerMonitor.Services;

namespace ServerMonitor.ViewModels;

public class EditProfileViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;
    private string _nombre;
    private string _email;
    private string _currentPassword;
    private string _newPassword;
    private string _confirmNewPassword;
    private bool _isLoading;
    private string _errorMessage;
    private string _successMessage;
    private int _userId;

    public string Nombre
    {
        get => _nombre;
        set
        {
            _nombre = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            _currentPassword = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public string NewPassword
    {
        get => _newPassword;
        set
        {
            _newPassword = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set
        {
            _confirmNewPassword = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanUpdate));
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            _successMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSuccess));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool CanUpdate =>
        !string.IsNullOrWhiteSpace(Nombre) ||
        !string.IsNullOrWhiteSpace(Email) ||
        (!string.IsNullOrWhiteSpace(NewPassword) && NewPassword == ConfirmNewPassword) ||
        !IsLoading;

    public EditProfileViewModel(UserService userService)
    {
        _userService = userService;
    }

    public async Task LoadCurrentUserAsync()
    {
        try
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user != null)
            {
                _userId = user.Id;
                Email = user.Email;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[v0] Load User Exception: {ex.Message}");
        }
    }

    public async Task<bool> UpdateProfileAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsLoading = true;

        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            if (NewPassword != ConfirmNewPassword)
            {
                ErrorMessage = "Las contraseñas nuevas no coinciden";
                IsLoading = false;
                return false;
            }
            if (NewPassword.Length < 6)
            {
                ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres";
                IsLoading = false;
                return false;
            }
        }

        var result = await _userService.UpdateProfileAsync(
            userId: _userId,
            nombre: string.IsNullOrWhiteSpace(Nombre) ? null : Nombre.Trim(),
            email: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            currentPassword: string.IsNullOrWhiteSpace(CurrentPassword) ? null : CurrentPassword,
            newPassword: string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword
        );

        IsLoading = false;

        if (result.Success)
        {
            SuccessMessage = result.Message;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
            return true;
        }
        else
        {
            ErrorMessage = result.Message;
            return false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}