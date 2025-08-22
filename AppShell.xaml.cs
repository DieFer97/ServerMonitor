using Microsoft.Maui.Controls;

namespace ServerMonitor;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(GraphicsPage), typeof(GraphicsPage));
    }
}