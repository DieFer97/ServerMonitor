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
    [ObservableProperty] private DateTime _startDate;
    [ObservableProperty] private DateTime _endDate;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private string _selectedLevel = "Normales";
    private const int PageSize = 5;

    public IEnumerable<string> Levels => new[] { "Normales", "Críticos" };

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage * PageSize < FilteredCount;
    public string PageInfo => $"Pág. {CurrentPage}";

    private List<SensorData> FilteredData = new();
    private int FilteredCount => FilteredData.Count;

    public SensorViewModel(SensorService sensorService)
    {
        _sensorService = sensorService;
        _startDate = new DateTime(2025, 8, 16);
        _endDate = DateTime.Now.Date;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        IsBusy = true;
        var data = await Task.Run(() => _sensorService.GetSensorDataAsync());
        SensorData = data ?? new List<SensorData>();
        FilterByDateCommand.Execute(null);
        IsBusy = false;
    }

    [RelayCommand]
    private void FilterByDate()
    {
        var query = SensorData
            .Where(s => s.Timestamp >= StartDate.Date && s.Timestamp < EndDate.Date.AddDays(1));

        if (SelectedLevel == "Críticos")
            query = query.Where(s => s.IsAlarm);
        else
            query = query.Where(s => !s.IsAlarm);

        FilteredData = query.OrderByDescending(x => x.Timestamp).ToList();
        CurrentPage = 1;
        RefreshPage();
    }

    [RelayCommand] private void NextPage() { if (HasNextPage) { CurrentPage++; RefreshPage(); } }
    [RelayCommand] private void PreviousPage() { if (HasPreviousPage) { CurrentPage--; RefreshPage(); } }

    private void RefreshPage()
    {
        var paged = FilteredData
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PagedData.Clear();
            foreach (var item in paged) PagedData.Add(item);
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        });
    }
}