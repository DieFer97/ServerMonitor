using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using ServerMonitor.Models;
using ServerMonitor.Services;
using SkiaSharp;

namespace ServerMonitor.ViewModels;

public partial class GraphicsViewModel : ObservableObject
{
    private readonly SensorService _sensorService;

    [ObservableProperty] private List<SensorData> _sensorData = new();
    [ObservableProperty] private DateTime _selectedDate;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _selectedSensor = "Temperatura";
    [ObservableProperty] private Chart _chart;

    public IEnumerable<string> Sensors => new[] { "Temperatura", "Flama" };

    private List<SensorData> _filteredData = new();

    public GraphicsViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
        _selectedDate = DateTime.Now.Date;

        _ = Task.Run(async () => await LoadDataAsync());
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var data = await _sensorService.GetSensorDataAsync();
            SensorData = data ?? new List<SensorData>();
            FilterByDateCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando datos: {ex.Message}");
            SensorData = new List<SensorData>();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void FilterByDate()
    {
        var startOfDay = SelectedDate.Date;
        var endOfDay = SelectedDate.Date.AddDays(1);

        var query = SensorData
            .Where(s => s.Timestamp >= startOfDay && s.Timestamp < endOfDay);

        _filteredData = query.OrderByDescending(x => x.Timestamp).ToList();
        UpdateChart();
    }

    private void UpdateChart()
    {
        var entries = new List<ChartEntry>();

        if (SelectedSensor == "Temperatura")
        {
            entries = _filteredData.Select(d => new ChartEntry(NormalizeTemperature((float)d.Temperature))
            {
                Label = d.Timestamp.ToString("dd-MM-yyyy"),
                ValueLabel = d.Temperature.ToString("F1") + "°C",
                Color = d.IsAlarm ? SKColor.Parse("#FF0000") : SKColor.Parse("#00FF00")
            }).ToList();
            Chart = new BarChart
            {
                Entries = entries,
                LabelTextSize = 12,
                ValueLabelTextSize = 12,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                BackgroundColor = SKColor.Parse("#F5F6FA"),
                Margin = 20,
                AnimationDuration = TimeSpan.FromSeconds(1),
            };
        }
        else if (SelectedSensor == "Flama")
        {
            entries = _filteredData.Select(d => new ChartEntry(NormalizeLightLevel((float)d.LightLevel))
            {
                Label = d.Timestamp.ToString("dd-MM-yyyy"),
                ValueLabel = d.LightLevel.ToString(),
                Color = d.IsAlarm ? SKColor.Parse("#FF0000") : SKColor.Parse("#00FF00")
            }).ToList();
            Chart = new BarChart
            {
                Entries = entries,
                LabelTextSize = 12,
                ValueLabelTextSize = 12,
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                BackgroundColor = SKColor.Parse("#F5F6FA"),
                Margin = 20,
                MaxValue = 255,
                MinValue = 0,
                AnimationDuration = TimeSpan.FromSeconds(1),
            };
        }
    }

    private float NormalizeTemperature(float temperature)
    {
        const float maxTemp = 100.0f;
        float normalized = (temperature / maxTemp) * 255.0f;
        return Math.Clamp(normalized, 0f, 255f);
    }

    private float NormalizeLightLevel(float lightLevel)
    {
        const float maxLight = 5000.0f;
        float normalized = (lightLevel / maxLight) * 255.0f;
        return Math.Clamp(normalized, 0f, 255f);
    }
}
