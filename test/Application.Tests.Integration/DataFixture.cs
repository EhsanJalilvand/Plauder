using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Settings;
using InfrastructureShare.Services;
using Microsoft.Extensions.Options;
using Moq;
using Server.Application.Services;
using Share.Tests.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tests.Integration
{
    public class DataFixture
    {
        private IServerService _serverService;
        private IClientService _clientService;
        private IClientService _clientService2;
        private IServerMessageProvider _ServerMessageProvider;
        private IMessageResolver _ServerMessageResolver;
        private IMessageResolver _ClientMessageResolver;
        private static readonly object _lock = new object();
        public void StartNewServerAndClient()
        {
            var config = ServerSettingBuilder.Build();
            _serverService = ServerBuilder.Build(config.Value, (messageResolver) => { _ServerMessageResolver = messageResolver; }, (serverMessageProvider) => { _ServerMessageProvider = serverMessageProvider; });
            Task.Factory.StartNew(() =>
            {
                _serverService.StartService((a) => { });
            });
            _clientService = ClientBuilder.Build(config.Value, (messageprovider) => { _ClientMessageResolver = messageprovider; });

            _clientService2 = ClientBuilder.Build(config.Value, (messageprovider) => { _ClientMessageResolver = messageprovider; });
        }
        public IMessageResolver ServerMessageResolver { get { return _ServerMessageResolver; } }
        public IServerMessageProvider ServerMessageProvider { get { return _ServerMessageProvider; } }
        public IMessageResolver ClientMessageResolver { get { return _ClientMessageResolver; } }
        public IServerService ServerSrvice { get { return _serverService; } }
        public IClientService ClientSrvice { get { return _clientService; } }
        public IClientService ClientSrvice2 { get { return _clientService2; } }
    }
}
