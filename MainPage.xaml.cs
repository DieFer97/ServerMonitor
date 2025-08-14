using ServerMonitor.Models;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public void Refresh(SensorData data) =>
        (BindingContext as MainViewModel)?.UpdateData(data);
}