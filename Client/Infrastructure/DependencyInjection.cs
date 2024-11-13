using ApplicationShare.Services;
using Client.Application.Services;
using InfrastructureClient.Services;
using InfrastructureShare.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.InfrastructureClient
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IClientMessageProvider, ClientMessageProvider>();
            services.AddSingleton<ISocketClientProvider, SocketClientProvider>();
            return services;
        }

    }
}
