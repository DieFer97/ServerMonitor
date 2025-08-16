using Microsoft.Maui.Controls;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

public partial class MainPage : ContentPage
{
    public MainPage(SensorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}