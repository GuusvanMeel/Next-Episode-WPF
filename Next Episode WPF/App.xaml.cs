using DAL;
using Interfaces.Entities;
using Interfaces.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service;
using System;
using System.IO;
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

            string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
            Xabe.FFmpeg.FFmpeg.SetExecutablesPath(ffmpegPath);

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            var HomeWindow = ServiceProvider.GetRequiredService<HomeWindow>();
            HomeWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register your services and repos here
            services.AddSingleton<ILoggerService, FileLogger>(); // example
            services.AddSingleton<ISettingsRepo, SettingsJSONRepo>(); // example
            services.AddSingleton<IShowRepo, ShowJSONRepo>();
            services.AddSingleton<IActivityLogRepo, ActivityLogJSONRepo>();
            services.AddSingleton<ShowService>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<EpisodeService>();
            services.AddSingleton<HomeWindow>();
            services.AddSingleton<ActivityService>();
        }
    }
}
