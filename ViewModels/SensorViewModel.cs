using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMonitor.Models;
using ServerMonitor.Services;
using System.Collections.ObjectModel;
namespace ServerMonitor.ViewModels;
public partial class SensorViewModel : ObservableObject
{
    private readonly SensorService _sensorService;
    // PROPIEDADES ORIGINALES - NO MODIFICADAS
    [ObservableProperty]
    private List<SensorData> _sensorData;
    // NUEVAS PROPIEDADES AGREGADAS PARA EL FILTRO
    [ObservableProperty]
    private ObservableCollection<SensorData> _filteredSensorData;
    [ObservableProperty]
    private DateTime _startDate;
    [ObservableProperty]
    private DateTime _endDate;
    public SensorViewModel(SensorService sensorService)
    {
        // CÓDIGO ORIGINAL - NO MODIFICADO
        _sensorService = sensorService;
        _sensorData = new List<SensorData>();
        // NUEVAS INICIALIZACIONES AGREGADAS
        _filteredSensorData = new ObservableCollection<SensorData>();
        // Inicializar con un rango más amplio para mostrar todos los datos
        _endDate = DateTime.Now.AddDays(1); // Un día en el futuro para asegurar que incluye hoy
        _startDate = DateTime.Now.AddDays(-365); // Un año atrás para mostrar todos los datos
        // CÓDIGO ORIGINAL - NO MODIFICADO
        LoadDataCommand.Execute(null);
    }
    // MÉTODO ORIGINAL - NO MODIFICADO
    [RelayCommand]
    private async Task LoadData()
    {
        SensorData = await _sensorService.GetSensorDataAsync();
        // NUEVA FUNCIONALIDAD AGREGADA - Solo se ejecuta si hay datos filtrados
        if (FilteredSensorData != null)
        {
            UpdateFilteredData();
        }
    }
    // NUEVO MÉTODO AGREGADO PARA EL FILTRO
    [RelayCommand]
    private void FilterByDate()
    {
        UpdateFilteredData();
    }
    // MÉTODO PRIVADO PARA MANEJAR LA LÓGICA DE FILTRADO
    private void UpdateFilteredData()
    {
        if (SensorData == null || FilteredSensorData == null) return;
        // Si no hay fechas válidas, mostrar todos los datos
        if (StartDate == default || EndDate == default)
        {
            FilteredSensorData.Clear();
            foreach (var item in SensorData)
            {
                FilteredSensorData.Add(item);
            }
            return;
        }
        var filteredData = SensorData.Where(sensor =>
            sensor.Timestamp.Date >= StartDate.Date &&
            sensor.Timestamp.Date <= EndDate.Date)
            .OrderByDescending(x => x.Timestamp) // Ordenar por fecha descendente
            .ToList();
        FilteredSensorData.Clear();
        foreach (var item in filteredData)
        {
            FilteredSensorData.Add(item);
        }
        // Si no hay datos filtrados, mostrar todos los datos disponibles
        if (FilteredSensorData.Count == 0 && SensorData.Count > 0)
        {
            foreach (var item in SensorData.OrderByDescending(x => x.Timestamp))
            {
                FilteredSensorData.Add(item);
            }
        }
    }
    // MÉTODOS OPCIONALES - Se ejecutan solo si las fechas cambian
    partial void OnStartDateChanged(DateTime value)
    {
        if (FilteredSensorData != null && SensorData != null && SensorData.Count > 0)
        {
            UpdateFilteredData();
        }
    }
    partial void OnEndDateChanged(DateTime value)
    {
        if (FilteredSensorData != null && SensorData != null && SensorData.Count > 0)
        {
            UpdateFilteredData();
        }
    }
}