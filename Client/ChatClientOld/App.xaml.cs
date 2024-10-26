using ApplicationShare.Settings;
using ChatClient.ViewModels;
using Client.Application.Services;
using InfrastructureClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Share.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var config = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<IClientService, ClientService>();
                    services.RegisterSharedServices();
                    services.Configure<ServerSetting>(config.GetSection("Server"));
                }).Build();

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
        protected async override void OnExit(ExitEventArgs e)
        {
            await _host.Services.GetService<IClientService>()?.CloseSession();
            _host.Dispose();
            base.OnExit(e);
        }

    }
}
