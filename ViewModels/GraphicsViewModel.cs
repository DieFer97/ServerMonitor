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
    [ObservableProperty] private string _selectedLevel = "Normales";
    [ObservableProperty] private Chart _chart;

    public IEnumerable<string> Levels => new[] { "Normales", "Críticos" };

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

        if (SelectedLevel == "Críticos")
            query = query.Where(s => s.IsAlarm);
        else
            query = query.Where(s => !s.IsAlarm);

        _filteredData = query.OrderByDescending(x => x.Timestamp).ToList();
        UpdateChart();
    }

    private void UpdateChart()
    {
        var entries = _filteredData.Select(d => new ChartEntry(d.Temperature)
        {
            Label = d.Timestamp.ToString("HH:mm"),
            ValueLabel = d.Temperature.ToString("F1"),
            Color = d.IsAlarm ? SKColor.Parse("#FF0000") : SKColor.Parse("#00FF00")
        }).ToArray();

        Chart = new BarChart
        {
            Entries = entries,
            LabelTextSize = 12,
            ValueLabelTextSize = 10,
            LabelOrientation = Orientation.Horizontal,
            ValueLabelOrientation = Orientation.Horizontal,
            BackgroundColor = SKColor.Parse("#FFFFFF"),
            Margin = 20,
            AnimationDuration = TimeSpan.FromSeconds(1)
        };
    }
}