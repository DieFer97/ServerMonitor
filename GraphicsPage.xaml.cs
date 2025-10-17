using Microsoft.Maui.Controls;
using ServerMonitor.ViewModels;

namespace ServerMonitor;

public partial class GraphicsPage : ContentPage
{
    private readonly GraphicsViewModel _viewModel;

    public GraphicsPage(GraphicsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnPageAppearing(object sender, EventArgs e)
    {
        await Task.Delay(100);

        if (_viewModel.LoadDataCommand.CanExecute(null))
        {
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }
}