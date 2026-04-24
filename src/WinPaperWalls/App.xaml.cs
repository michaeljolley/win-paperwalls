using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System.Threading;
using WinPaperWalls.Services;

namespace WinPaperWalls;

public partial class App : Application
{
    private static Mutex? _instanceMutex;
    private IHost? _host;

    public App()
    {
        InitializeComponent();
    }

    public static IServiceProvider Services => ((App)Current)._host!.Services;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Single instance check
        _instanceMutex = new Mutex(true, "WinPaperWalls_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            // Another instance is running
            Exit();
            return;
        }

        // Build the host with DI container
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<ISettingsService, SettingsService>();
                
                // Register window
                services.AddSingleton<MainWindow>();
            })
            .Build();

        // Start the host
        _host.Start();

        // Create and activate main window
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }
}
