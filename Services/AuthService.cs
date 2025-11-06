using System.Text;
using System.Text.Json;
using ServerMonitor.Models;

namespace ServerMonitor.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://iot-monitoring-backend-w51f.onrender.com/api/auth";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string Message, User User)> RegisterAsync(string email, string nombre, string password)
    {
        try
        {
            var payload = new { email, nombre, password };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{ApiUrl}/register", content);
            var responseText = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[v0] Register Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[v0] Register Response Body: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(responseText))
                {
                    return (false, "Respuesta vacía del servidor", null);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // CAMBIO: Deserializar como RegisterResponse que contiene el objeto user
                var registerResponse = JsonSerializer.Deserialize<AuthResponse>(responseText, options);

                if (registerResponse?.User == null || registerResponse.User.Id == 0)
                {
                    return (false, "Error: Usuario no válido en respuesta", null);
                }

                await SecureStorage.SetAsync("userId", registerResponse.User.Id.ToString());
                await SecureStorage.SetAsync("userEmail", registerResponse.User.Email);

                return (true, registerResponse.Message ?? "Registro exitoso", registerResponse.User);
            }
            else
            {
                return (false, "Error en el registro: " + responseText, null);
            }
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"[v0] JSON Error: {jsonEx.Message}");
            return (false, "Error al parsear respuesta: " + jsonEx.Message, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[v0] Register Exception: {ex.Message}\n{ex.StackTrace}");
            return (false, "Error: " + ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message, User User)> LoginAsync(string email, string password)
    {
        try
        {
            var payload = new { email, password };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{ApiUrl}/login", content);
            var responseText = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"[v0] Login Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[v0] Login Response Body: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(responseText))
                {
                    return (false, "Respuesta vacía del servidor", null);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loginResponse = JsonSerializer.Deserialize<AuthResponse>(responseText, options);
                var user = loginResponse?.User;

                if (user == null || user.Id == 0)
                {
                    return (false, "Error: Usuario no válido en respuesta", null);
                }

                await SecureStorage.SetAsync("userId", user.Id.ToString());
                await SecureStorage.SetAsync("userEmail", user.Email);

                return (true, "Login exitoso", user);
            }
            else
            {
                return (false, "Credenciales inválidas", null);
            }
        }
        catch (JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"[v0] JSON Error: {jsonEx.Message}");
            return (false, "Error al parsear respuesta: " + jsonEx.Message, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[v0] Login Exception: {ex.Message}\n{ex.StackTrace}");
            return (false, "Error: " + ex.Message, null);
        }
    }

    internal async Task<bool> IsLoggedInAsync()
    {
        try
        {
            var userId = await SecureStorage.GetAsync("userId");
            return !string.IsNullOrEmpty(userId);
        }
        catch
        {
            return false;
        }
    }

    private class AuthResponse
    {
        public string Message { get; set; }
        public User User { get; set; }
    }
}