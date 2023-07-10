using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Pinpoint.Core;
using Pinpoint.Win.ViewModels;
using Pinpoint.Win.Views;

namespace Pinpoint.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        public IServiceProvider Services { get; } = ConfigureServices();

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SettingsWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<PluginTabItem>();
            services.AddTransient<PluginTabItemViewModel>();
            services.AddSingleton<PluginEngine>();
            return services.BuildServiceProvider();
        }

        public new MainWindow MainWindow => Services.GetService<MainWindow>();

        public SettingsWindow SettingsWindow => Services.GetService<SettingsWindow>();

        public MainViewModel MainViewModel => Services.GetService<MainViewModel>();

        public SettingsViewModel SettingsViewModel => Services.GetService<SettingsViewModel>();

        public PluginTabItemViewModel PluginTabItemViewModel => Services.GetService<PluginTabItemViewModel>();

        public PluginEngine PluginEngine => Services.GetService<PluginEngine>();

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Services.GetService<MainWindow>()?.Show();
        }
    }
}
