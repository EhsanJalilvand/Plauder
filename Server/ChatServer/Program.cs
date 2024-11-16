// See https://aka.ms/new-console-template for more information
using DomainShare.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Application.Services;
using Server.InfrastructureServer;
using Share.Infrastructure;


var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json",optional:false,reloadOnChange:true);   
var config=builder.Build();


var serviceProvider = new ServiceCollection()
           .Configure<ServerSetting>(config.GetSection("Server"))
           .AddMemoryCache()
           .RegisterSharedServices()
           .RegisterServices()
           .BuildServiceProvider();


var service= serviceProvider.GetService<IServerService>();
service.StartService((a) => { });
