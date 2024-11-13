using Client.Application.Services;
using Client.InfrastructureClient;
using DomainShare.Settings;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Services;
using Share.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Tests.Builder
{
    public static class ClientBuilder
    {
        public static IClientService Build()
        {

            var config = ServerSettingBuilder.Build().Value;
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


            var service = serviceProvider.GetService<IClientService>();

            return service;
        }
    }
}
