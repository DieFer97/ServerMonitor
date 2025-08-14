using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using ServerMonitor.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace ServerMonitor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _temperature;

        [ObservableProperty]
        private int _light;

        [ObservableProperty]
        private bool _alarm;

        public ObservableCollection<ChartEntry> TempEntries { get; } = new();
        public ObservableCollection<ChartEntry> LightEntries { get; } = new();

        public void UpdateData(SensorData data)
        {
            Temperature = data.Temperature;
            Light = data.Light;
            Alarm = data.Alarm;

            TempEntries.Add(new ChartEntry((float)data.Temperature)
            {
                Label = data.TimeStamp.ToString("HH:mm:ss"),
                ValueLabel = data.Temperature.ToString("0.0"),
                Color = SKColors.Red
            });
            LightEntries.Add(new ChartEntry(data.Light)
            {
                Label = data.TimeStamp.ToString("HH:mm:ss"),
                ValueLabel = data.Light.ToString(),
                Color = SKColors.Orange
            });

            if (TempEntries.Count > 30) TempEntries.RemoveAt(0);
            if (LightEntries.Count > 30) LightEntries.RemoveAt(0);
        }
    }
}