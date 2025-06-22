using DAL;
using Interfaces.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Next_Episode_WPF;
using Service;
using System.Windows;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register your services and repos here
        services.AddSingleton<ILoggerService, FileLogger>(); // example
        services.AddSingleton<IShowRepo, ShowJSONRepo>();
        services.AddSingleton<ShowService>();
        services.AddSingleton<EpisodeService>();
        services.AddSingleton<MainWindow>(); // important
    }
}
