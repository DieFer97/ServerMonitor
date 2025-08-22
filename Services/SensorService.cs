using System.Net.Http.Json;
using ServerMonitor.Models;

namespace ServerMonitor.Services;

public class SensorService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "http://10.127.212.163:3000/api/iot/data";

    public SensorService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SensorData>> GetSensorDataAsync(DateTime? fecha = null)
    {
        try
        {
            var url = ApiUrl;
            if (fecha.HasValue)
                url += $"?fecha={fecha.Value:yyyy-MM-dd}&limit=500";
            else
                url += "?limit=500";

            var response = await _httpClient.GetFromJsonAsync<List<SensorData>>(url);
            return response ?? new List<SensorData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<SensorData>();
        }
    }
}