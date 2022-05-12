﻿using Keystrokes.Views;
using Keystrokes.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using KeystrokesData;

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

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    ConfigureServices(services);
                })
                .Build();
            _host.Start();

            MainWindowFrame.Content = _host.Services.GetRequiredService<KeystrokeView>();
            NavBtn0.IsChecked = true;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<KeystrokeView>();
            services.AddTransient<IKeystrokeService, KeystrokeService>();
            services.AddTransient<IGraphService, GraphService>();
            services.AddDbContext<KeystrokesDbContext>(options =>
            {
                options.UseNpgsql("Server=localhost;Database=keystrokes_dev;Port=5432;User Id=postgres;Password=12345");
            });
        }

        private void ChangeToKeystrokesClicked(object sender, RoutedEventArgs e)
        {
            MainWindowFrame.Content = _host.Services.GetRequiredService<KeystrokeView>();
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