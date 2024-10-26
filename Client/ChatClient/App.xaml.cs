using ApplicationShare.Settings;
using ChatClient.ViewModels;
using Client.Application.Services;
using InfrastructureClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Share.Infrastructure;
namespace ChatClient
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = serviceProvider.GetService<AppShell>();// new AppShell();
        }


    }
}
