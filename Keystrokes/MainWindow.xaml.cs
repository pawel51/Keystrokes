using Keystrokes.Views;
using Keystrokes.Services;
using Keystrokes.Services.Impl;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using KeystrokesData;
using Keystrokes.Services.Interfaces;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace Keystrokes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IHost _host;
        public MainWindow()
        {
            InitializeComponent();

            
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(@"C:\logs\keystrokes\log.log").WriteTo.Console().CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    ConfigureServices(services);
                })
                .UseSerilog()
                .Build();
            _host.Start();

            MainWindowFrame.Content = _host.Services.GetRequiredService<KeystrokeView>();

            NavBtn0.IsChecked = true;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<KeystrokeView>();
            services.AddSingleton<ClassificationView>();
            services.AddTransient<IKeystrokeService, KeystrokeService>();
            services.AddTransient<IGraphService, GraphService>();
            services.AddTransient<IKnnClassificatorService, KnnClassificationService>();
            services.AddDbContext<KeystrokesDbContext>(options =>
            {
                options.UseNpgsql("Server=localhost;Database=keystrokes_dev;Port=5432;User Id=postgres;Password=12345");
            });
        }

        private void ChangeToKeystrokesClicked(object sender, RoutedEventArgs e)
        {
            NavBtn0.IsChecked = true;
            NavBtn1.IsChecked = false;
            MainWindowFrame.Content = _host.Services.GetRequiredService<KeystrokeView>();
        }

        private void ChangeToClassificationClicked(object sender, RoutedEventArgs e)
        {
            NavBtn0.IsChecked = false;
            NavBtn1.IsChecked = true;
            MainWindowFrame.Content = _host.Services.GetRequiredService<ClassificationView>();
        }

        protected override async void OnClosed(EventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnClosed(e);
        }

    }
}
