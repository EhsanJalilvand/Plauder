using ChatClient.ViewModels;
using Client.Application.Services;
using DomainShare.Settings;
using InfrastructureClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Share.Infrastructure;
using System.Reflection;
namespace ChatClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var config = configurationBuilder.Build();
            //config.GetSection("Server").Bind(new ServerSetting());

            var builder = MauiApp.CreateBuilder();


            var serverSetting = config.GetRequiredSection("Server").Get<ServerSetting>();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Configuration.AddConfiguration(config);
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<IClientService, ClientService>();
            builder.Services.RegisterSharedServices();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Configuration.GetSection("Server").Bind(serverSetting);
            builder.Services.Configure<ServerSetting>(option => { option.Ip = serverSetting.Ip; option.Port = serverSetting.Port; });




#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
