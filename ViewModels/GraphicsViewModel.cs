using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microcharts;
using ServerMonitor.Models;
using ServerMonitor.Services;
using SkiaSharp;
using Microsoft.Maui.Controls;

namespace ServerMonitor.ViewModels;

public partial class GraphicsViewModel : ObservableObject
{
    private readonly SensorService _sensorService;

    [ObservableProperty] private List<SensorData> _sensorData = new();
    [ObservableProperty] private DateTime _selectedDate;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _selectedSensor = "Temperatura";
    [ObservableProperty] private Chart _chart;
    [ObservableProperty] private string _timeWindowInfo = "";
    [ObservableProperty] private string _selectedChartType = "Line";

    [ObservableProperty] private ScrollBarVisibility _scrollBarVisibility = ScrollBarVisibility.Default;
    [ObservableProperty] private double _chartMinWidth = 1000;
    [ObservableProperty] private double _chartWidth = 900;
    [ObservableProperty] private LayoutOptions _chartHorizontalOptions = LayoutOptions.Fill;

    public IEnumerable<string> Sensors => new[] { "Temperatura", "Flama" };
    public IEnumerable<string> ChartTypes => new[] { "Line", "Bar", "Point", "Donut" };

    private List<SensorData> _filteredData = new();
    private IDispatcherTimer _realtimeTimer;

    public GraphicsViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
        _selectedDate = DateTime.Now.Date;

        InitializeDefaultChart();

        InitializeRealtimeTimer();
    }

    private void InitializeDefaultChart()
    {
        var defaultEntries = new List<ChartEntry>
        {
            new ChartEntry(0)
            {
                Label = "Cargando...",
                Color = SKColor.Parse("#CCCCCC"),
                ValueLabel = "0"
            }
        };

        _chart = new LineChart
        {
            Entries = defaultEntries,
            BackgroundColor = SKColor.Parse("#F8F9FA"),
            LabelTextSize = 10,
            Margin = 0
        };
    }

    private void InitializeRealtimeTimer()
    {
        _realtimeTimer = Application.Current?.Dispatcher.CreateTimer();
        if (_realtimeTimer != null)
        {
            _realtimeTimer.Interval = TimeSpan.FromSeconds(30);
            _realtimeTimer.Tick += async (s, e) => await RefreshDataAsync();
            _realtimeTimer.Start();
        }
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            var data = await _sensorService.GetSensorDataAsync();
            if (data != null)
            {
                SensorData = data;
                FilterByDateCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refrescando datos en tiempo real: {ex.Message}");
        }
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
    private async Task FilterByDate()
    {
        try
        {
            IsBusy = true;

            var data = await _sensorService.GetSensorDataAsync(SelectedDate);
            SensorData = data ?? new List<SensorData>();

            var startOfDay = SelectedDate.Date;
            var endOfDay = SelectedDate.Date.AddDays(1);

            _filteredData = SensorData
                .Where(s => s.Timestamp >= startOfDay && s.Timestamp < endOfDay)
                .OrderBy(x => x.Timestamp)
                .ToList();

            UpdateChart();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error filtering by date: {ex.Message}");
            _filteredData = new List<SensorData>();
            UpdateChart();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateChart()
    {
        try
        {
            var entries = new List<ChartEntry>();

            UpdateChartVisibility();

            if (_filteredData == null || _filteredData.Count == 0)
            {
                entries.Add(new ChartEntry(0)
                {
                    Label = "Sin datos",
                    Color = SKColor.Parse("#CCCCCC"),
                    ValueLabel = "0"
                });

                Chart = new LineChart
                {
                    Entries = entries,
                    BackgroundColor = SKColor.Parse("#F8F9FA"),
                    LabelTextSize = 10,
                    Margin = 0
                };
                TimeWindowInfo = "Sin datos disponibles";
                return;
            }

            var dataToDisplay = _filteredData;

            var firstTime = dataToDisplay.First().Timestamp;
            var lastTime = dataToDisplay.Last().Timestamp;
            TimeWindowInfo = $"Datos del día: {firstTime:HH:mm:ss} - {lastTime:HH:mm:ss} ({dataToDisplay.Count} registros)";

            if (SelectedChartType == "Donut")
            {
                if (SelectedSensor == "Temperatura")
                {
                    var normalData = dataToDisplay.Where(d => !d.IsAlarm).ToList();
                    var alarmData = dataToDisplay.Where(d => d.IsAlarm).ToList();

                    if (normalData.Any())
                    {
                        entries.Add(new ChartEntry((float)normalData.Average(d => d.Temperature))
                        {
                            Label = "Normal",
                            Color = SKColor.Parse("#4CAF50"),
                            ValueLabel = $"{normalData.Average(d => d.Temperature):F1}°C"
                        });
                    }

                    if (alarmData.Any())
                    {
                        entries.Add(new ChartEntry((float)alarmData.Average(d => d.Temperature))
                        {
                            Label = "Alarma",
                            Color = SKColor.Parse("#FF4444"),
                            ValueLabel = $"{alarmData.Average(d => d.Temperature):F1}°C"
                        });
                    }

                    if (!entries.Any())
                    {
                        entries.Add(new ChartEntry(1)
                        {
                            Label = "Sin datos",
                            Color = SKColor.Parse("#CCCCCC"),
                            ValueLabel = "0°C"
                        });
                    }
                }
                else if (SelectedSensor == "Flama")
                {
                    var normalData = dataToDisplay.Where(d => !d.IsAlarm).ToList();
                    var alarmData = dataToDisplay.Where(d => d.IsAlarm).ToList();

                    if (normalData.Any())
                    {
                        entries.Add(new ChartEntry((float)normalData.Average(d => d.LightLevel))
                        {
                            Label = "Normal",
                            Color = SKColor.Parse("#2196F3"),
                            ValueLabel = $"{normalData.Average(d => d.LightLevel):F0} lux"
                        });
                    }

                    if (alarmData.Any())
                    {
                        entries.Add(new ChartEntry((float)alarmData.Average(d => d.LightLevel))
                        {
                            Label = "Alarma",
                            Color = SKColor.Parse("#FF6B35"),
                            ValueLabel = $"{alarmData.Average(d => d.LightLevel):F0} lux"
                        });
                    }

                    if (!entries.Any())
                    {
                        entries.Add(new ChartEntry(1)
                        {
                            Label = "Sin datos",
                            Color = SKColor.Parse("#CCCCCC"),
                            ValueLabel = "0 lux"
                        });
                    }
                }
            }
            else
            {
                if (SelectedSensor == "Temperatura")
                {
                    entries = dataToDisplay.Select(d => new ChartEntry(d.Temperature)
                    {
                        Label = d.Timestamp.ToString("HH:mm:ss"),
                        Color = d.IsAlarm ? SKColor.Parse("#FF4444") : SKColor.Parse("#4CAF50"),
                        ValueLabel = d.Temperature.ToString("F1") + "°C"
                    }).ToList();
                }
                else if (SelectedSensor == "Flama")
                {
                    entries = dataToDisplay.Select(d => new ChartEntry((float)d.LightLevel)
                    {
                        Label = d.Timestamp.ToString("HH:mm:ss"),
                        Color = d.IsAlarm ? SKColor.Parse("#FF6B35") : SKColor.Parse("#2196F3"),
                        ValueLabel = d.LightLevel.ToString() + " lux"
                    }).ToList();
                }
            }

            switch (SelectedChartType)
            {
                case "Bar":
                    Chart = new BarChart
                    {
                        Entries = entries.Any() ? entries : new List<ChartEntry> { new ChartEntry(0) { Label = "Sin datos", Color = SKColor.Parse("#CCCCCC") } },
                        LabelTextSize = 10,
                        LabelOrientation = Orientation.Horizontal,
                        BackgroundColor = SKColor.Parse("#F8F9FA"),
                        Margin = 0,
                        ValueLabelOrientation = Orientation.Horizontal,
                        ValueLabelTextSize = 10,
                        MaxValue = 100,
                        MinValue = 0
                    };
                    break;
                case "Point":
                    Chart = new PointChart
                    {
                        Entries = entries.Any() ? entries : new List<ChartEntry> { new ChartEntry(0) { Label = "Sin datos", Color = SKColor.Parse("#CCCCCC") } },
                        LabelTextSize = 10,
                        LabelOrientation = Orientation.Horizontal,
                        BackgroundColor = SKColor.Parse("#F8F9FA"),
                        Margin = 0,
                        PointSize = 10,
                        MaxValue = 100,
                        MinValue = 0
                    };
                    break;
                case "Donut":
                    Chart = new DonutChart
                    {
                        Entries = entries.Any() ? entries : new List<ChartEntry> { new ChartEntry(1) { Label = "Sin datos", Color = SKColor.Parse("#CCCCCC") } },
                        LabelTextSize = 10,
                        BackgroundColor = SKColor.Parse("#F8F9FA"),
                        Margin = 0,
                        MaxValue = 100,
                        MinValue = 0,
                        HoleRadius = 0.5f
                    };
                    break;
                case "Line":
                default:
                    Chart = new LineChart
                    {
                        Entries = entries.Any() ? entries : new List<ChartEntry> { new ChartEntry(0) { Label = "Sin datos", Color = SKColor.Parse("#CCCCCC") } },
                        LabelTextSize = 10,
                        LabelOrientation = Orientation.Horizontal,
                        BackgroundColor = SKColor.Parse("#F8F9FA"),
                        Margin = 0,
                        AnimationDuration = TimeSpan.FromSeconds(1),
                        MaxValue = 100,
                        MinValue = 0,
                        LineSize = 3,
                        PointSize = 6,
                        IsAnimated = true,
                    };
                    break;
            }

            UpdateChartVisibility();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error actualizando gráfico: {ex.Message}");

            var fallbackEntries = new List<ChartEntry>
            {
                new ChartEntry(0)
                {
                    Label = "Error",
                    Color = SKColor.Parse("#FF4444"),
                    ValueLabel = "Error"
                }
            };

            Chart = new LineChart
            {
                Entries = fallbackEntries,
                BackgroundColor = SKColor.Parse("#F8F9FA"),
                LabelTextSize = 10,
                Margin = 0
            };
            TimeWindowInfo = "Error al cargar gráfico";
        }
    }

    private void UpdateChartVisibility()
    {
        if (SelectedChartType == "Donut")
        {
            ScrollBarVisibility = ScrollBarVisibility.Never;
            ChartMinWidth = 0;
            ChartWidth = 370;
            ChartHorizontalOptions = LayoutOptions.Center;
        }
        else
        {
            ScrollBarVisibility = ScrollBarVisibility.Default;
            ChartMinWidth = 1000;
            ChartWidth = 900;
            ChartHorizontalOptions = LayoutOptions.Fill;
        }
    }
}