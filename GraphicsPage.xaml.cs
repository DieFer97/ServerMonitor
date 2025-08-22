using Microsoft.Maui.Controls;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

public partial class GraphicsPage : ContentPage
{
    public GraphicsPage(GraphicsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}