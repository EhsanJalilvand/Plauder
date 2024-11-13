using ApplicationShare.Services;
using InfrastructureShare.Services;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddSingleton<IMessageChunker, MessageChunker>();
            services.AddSingleton<IMessageQueueManager, MessageQueueManager>();
            services.AddSingleton<IMessageResolver, MessageResolver>();
          
           
            return services;
        }

    }
}
