using DAL;
using Interfaces.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service;
using System;
using System.Windows;
namespace Next_Episode_WPF // 👈 This must match App.xaml
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var testwindow = ServiceProvider.GetRequiredService<TestWindow>();
            testwindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register your services and repos here
            services.AddSingleton<ILoggerService, FileLogger>(); // example
            services.AddSingleton<ISettingsRepo, SettingsJSONRepo>(); // example
            services.AddSingleton<IShowRepo, ShowJSONRepo>();
            services.AddSingleton<ShowService>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<EpisodeService>();
            services.AddSingleton<MainWindow>(); // important
            services.AddSingleton<TestWindow>();
        }
    }
}
