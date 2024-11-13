using ApplicationShare.Services;
using InfrastructureShare.Services;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.InfrastructureServer
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<IServerMessageProvider, ServerMessageProvider>();
            services.AddSingleton<ISocketManager, SocketManager>();
            services.AddSingleton<ISocketServerProvider, SocketServerProvider>();
            return services;
        }

    }
}
