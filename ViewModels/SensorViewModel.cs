using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMonitor.Models;
using ServerMonitor.Services;
using System.Collections.ObjectModel;

namespace ServerMonitor.ViewModels;

public partial class SensorViewModel : ObservableObject
{
    private readonly SensorService _sensorService;

    [ObservableProperty] private List<SensorData> _sensorData = new();
    [ObservableProperty] private ObservableCollection<SensorData> _pagedData = new();
    [ObservableProperty] private DateTime _selectedDate;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private string _selectedLevel = "Normales";

    private const int PageSize = 7;
    private List<SensorData> _filteredData = new();
    private int FilteredCount => _filteredData.Count;

    public IEnumerable<string> Levels => new[] { "Normales", "Críticos" };
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage * PageSize < FilteredCount;
    public string PageInfo => $"Pág. {CurrentPage}";

    public SensorViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
        _selectedDate = DateTime.Now.Date;

        _ = Task.Run(async () => await LoadDataAsync());
    }

    [RelayCommand]
    private async Task LoadData()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            var data = await Task.Run(() => _sensorService.GetSensorDataAsync());
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

            var query = SensorData
                .Where(s => s.Timestamp >= startOfDay && s.Timestamp < endOfDay);

            if (SelectedLevel == "Críticos")
                query = query.Where(s => s.IsAlarm);
            else
                query = query.Where(s => !s.IsAlarm);

            _filteredData = query.OrderByDescending(x => x.Timestamp).ToList();
            CurrentPage = 1;
            RefreshPage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error filtering by date: {ex.Message}");
            _filteredData = new List<SensorData>();
            RefreshPage();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            RefreshPage();
        }
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            RefreshPage();
        }
    }

    private void RefreshPage()
    {
        var paged = _filteredData
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PagedData.Clear();
            foreach (var item in paged)
                PagedData.Add(item);

            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        });
    }
}