using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using ServerMonitor.Models;
using System.Diagnostics;

namespace ServerMonitor.Services;

public class SensorService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "https://iot-monitoring-backend-w51f.onrender.com/api/iot/data";

    public SensorService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SensorData>> GetSensorDataAsync(DateTime? fecha = null)
    {
        try
        {
            var url = ApiUrl;
            var userIdStr = await SecureStorage.GetAsync("userId");
            if (!string.IsNullOrEmpty(userIdStr))
                url = $"{ApiUrl}/user?userId={userIdStr}";

            if (fecha.HasValue)
                url += $"&fecha={fecha.Value:yyyy-MM-dd}&limit=500";
            else
                url += $"{(url.Contains("?") ? "&" : "?")}limit=500";

            var response = await _httpClient.GetFromJsonAsync<List<SensorData>>(url);
            return response ?? new List<SensorData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<SensorData>();
        }
    }


    private async Task PostSensorDataAsync(SensorData data)
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

            await _httpClient.PostAsync($"{ApiUrl}/submit", content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error posting sensor data: {ex.Message}");
        }
    }

    private readonly AuthService _authService;

    public SensorService(HttpClient httpClient, AuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }
    
}