using DomainShare.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.InfrastructureServer;
using Share.Infrastructure;
namespace Share.Tests.Builder
{
    public static class ServerBuilder
    {
        public static IServerService Build()
        {

            var config= ServerSettingBuilder.Build().Value;
            var serviceProvider = new ServiceCollection()
                       .Configure<ServerSetting>((a) =>
                       {
                           a.Ip = config.Ip;
                           a.Port = config.Port;
                           a.ChunkSize = config.ChunkSize;
                       })
                       .AddMemoryCache()
                       .RegisterSharedServices()
                       .RegisterServices()
                       .BuildServiceProvider();


            var service = serviceProvider.GetService<IServerService>();

            return service;
        }
    }
}
