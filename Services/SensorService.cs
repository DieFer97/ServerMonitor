using System.Net.Http.Json;
using ServerMonitor.Models;

namespace ServerMonitor.Services;

public class SensorService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "http://192.168.137.76:3000/api/iot/data";

    public SensorService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SensorData>> GetSensorDataAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<SensorData>>(ApiUrl);
            return response ?? new List<SensorData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener datos de sensores: {ex.Message}");
            return new List<SensorData>();
        }
    }
}