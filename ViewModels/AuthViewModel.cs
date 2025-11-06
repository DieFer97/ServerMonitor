using System.ComponentModel;
using System.Runtime.CompilerServices;
using ServerMonitor.Models;
using ServerMonitor.Services;

namespace ServerMonitor.ViewModels;

public class AuthViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;
    private string _email;
    private string _nombre;
    private string _password;
    private string _confirmPassword;
    private bool _isLoading;
    private string _errorMessage;

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(CanLogin));
        }
    }

    public string Nombre
    {
        get => _nombre;
        set
        {
            _nombre = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRegister));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(CanLogin));
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRegister));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRegister));
            OnPropertyChanged(nameof(CanLogin));
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

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email y contraseña son requeridos";
            return false;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var (success, message, user) = await _authService.LoginAsync(Email, Password);

        if (success)
        {
            ErrorMessage = string.Empty;
            IsLoading = false;
            return true;
        }
        else
        {
            ErrorMessage = message;
            IsLoading = false;
            return false;
        }
    }

    public async Task<bool> RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Nombre) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Todos los campos son requeridos";
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden";
            return false;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
            return false;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var (success, message, user) = await _authService.RegisterAsync(Email, Nombre, Password);

        if (success)
        {
            ErrorMessage = string.Empty;
            IsLoading = false;
            return true;
        }
        else
        {
            ErrorMessage = message;
            IsLoading = false;
            return false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool CanRegister =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Nombre) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        Password == ConfirmPassword &&
        Password.Length >= 6 &&
        !IsLoading;

    public bool CanLogin =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !IsLoading;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
}