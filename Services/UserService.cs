using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ServerMonitor.Models;

namespace ServerMonitor.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://iot-monitoring-backend-w51f.onrender.com/api/users";

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(
    int userId,
    string nombre = null,
    string email = null,
    string currentPassword = null,
    string newPassword = null)
    {
        try
        {
            var payload = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(nombre))
                payload["nombre"] = nombre.Trim();

            if (!string.IsNullOrWhiteSpace(email))
                payload["email"] = email.Trim();

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                    return (false, "La contraseña actual es requerida para cambiarla");

                payload["currentPassword"] = currentPassword;
                payload["newPassword"] = newPassword.Trim();
            }

            if (payload.Count == 0)
                return (false, "No se detectaron cambios para guardar");

            var json = JsonSerializer.Serialize(payload);
            System.Diagnostics.Debug.WriteLine($"[FINAL] Update Profile Payload: {json}");

            var url = "https://iot-monitoring-backend-w51f.onrender.com/api/users/update";
            var response = await _httpClient.PostAsJsonAsync(url, payload);

            var responseText = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[FINAL] Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[FINAL] Response Body: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                if (payload.ContainsKey("email"))
                    await SecureStorage.SetAsync("userEmail", email.Trim());

                return (true, "Perfil actualizado exitosamente");
            }
            else
            {
                return (false, "Error del servidor. Usa la web temporalmente.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FINAL] Exception: {ex.Message}");
            return (false, "Error de conexión: " + ex.Message);
        }
    }

    public async Task<User> GetCurrentUserAsync()
    {
        try
        {
            var userId = await SecureStorage.GetAsync("userId");
            var userEmail = await SecureStorage.GetAsync("userEmail");
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
            {
                return null;
            }
            return new User
            {
                Id = int.Parse(userId),
                Email = userEmail
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[v1] Get Current User Exception: {ex.Message}");
            return null;
        }
    }
}