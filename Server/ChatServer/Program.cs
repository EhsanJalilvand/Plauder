// See https://aka.ms/new-console-template for more information
using DomainShare.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Application.Services;
using Server.Infrastructure.Services;
using Share.Infrastructure;


var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json",optional:false,reloadOnChange:true);   
var config=builder.Build();


var serviceProvider = new ServiceCollection()
           .AddSingleton<IServerService, ServerService>()
           .Configure<ServerSetting>(config.GetSection("Server"))
           .AddMemoryCache()
           .RegisterSharedServices()
           .BuildServiceProvider();


var service= serviceProvider.GetService<IServerService>();
service.StartService((a) => { });
service.KeepLive();