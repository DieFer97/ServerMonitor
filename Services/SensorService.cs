using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using ServerMonitor.Models;
using System.Diagnostics;

namespace ServerMonitor.Services;

public class SensorService
{
    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;
    private const string ApiUrl = "https://iot-monitoring-backend-w51f.onrender.com/api/iot/data";

    public SensorService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<List<SensorData>> GetSensorDataAsync(DateTime? fecha = null)
    {
        try
        {
            var url = ApiUrl;

            if (fecha.HasValue)
            {
                url += $"?fecha={fecha.Value:yyyy-MM-dd}&limit=500";
            }
            else
            {
                url += "?limit=500";
            }

            Debug.WriteLine($"[v0] Fetching data from: {url}");

            var response = await _httpClient.GetFromJsonAsync<List<SensorData>>(url);

            Debug.WriteLine($"[v0] Data received: {response?.Count ?? 0} records");

            return response ?? new List<SensorData>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[v0] Error fetching sensor data: {ex.Message}");
            Debug.WriteLine($"[v0] StackTrace: {ex.StackTrace}");
            return new List<SensorData>();
        }
    }

    public async Task PostSensorDataAsync(SensorData data)
    {
        try
        {
            var userIdStr = await SecureStorage.GetAsync("userId");
            var userId = !string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var id) ? id : (int?)null;

            var payload = new
            {
                userId = userId,
                temperature = data.Temperature,
                lightLevel = data.LightLevel,
                isAlarm = data.IsAlarm,
                timestamp = DateTime.UtcNow
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{ApiUrl}/submit", content);

            Debug.WriteLine($"[v0] Post sensor data response: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[v0] Error posting sensor data: {ex.Message}");
        }
    }
}