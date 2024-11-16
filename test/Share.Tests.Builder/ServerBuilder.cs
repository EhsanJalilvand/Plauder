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
using ApplicationShare.Services;
using InfrastructureShare.Services;
namespace Share.Tests.Builder
{
    public static class ServerBuilder
    {
        public static IServerService Build(ServerSetting config, Action<IMessageResolver> messageResolverCallback,Action<IServerMessageProvider> serverMessageProviderCallback)
        {

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
            var messageResolver=serviceProvider.GetService<IMessageResolver>();
            var serverMessageProvider = serviceProvider.GetService<IServerMessageProvider>();
            messageResolverCallback(messageResolver);
            serverMessageProviderCallback(serverMessageProvider);    
            return service;
        }
    }
}
