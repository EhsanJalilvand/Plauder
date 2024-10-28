using Microsoft.Extensions.DependencyInjection;
using Share.Application.Services;
using Share.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterSharedServices(this IServiceCollection services)
        {
            services.AddSingleton<IServerMessageProvider, ServerMessageProvider>();
            services.AddSingleton<IClientMessageProvider, ClientMessageProvider>();
            return services;
        }

    }
}
