using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMonitor.Models;
using ServerMonitor.Services;

namespace ServerMonitor.ViewModels;

public partial class SensorViewModel : ObservableObject
{
    private readonly SensorService _sensorService;

    [ObservableProperty]
    private List<SensorData> _sensorData;

    public SensorViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
        _sensorData = new List<SensorData>();
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        SensorData = await _sensorService.GetSensorDataAsync();
    }
}