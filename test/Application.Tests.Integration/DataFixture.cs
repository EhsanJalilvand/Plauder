using Client.Application.Services;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using Server.Application.Services;
using Share.Tests.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tests.Integration
{
    public class DataFixture : IDisposable
    {
        private readonly IOptions<ServerSetting> _serverSettingOptions;
        private readonly IServerService _serverService;
        private readonly IClientService _clientService;
        public DataFixture()
        {
            _serverSettingOptions = ServerSettingBuilder.Build();
            _serverService=ServerBuilder.Build();
            _clientService=ClientBuilder.Build();
        }
        public IOptions<ServerSetting> ServerSettingOption { get { return _serverSettingOptions; } }
        public IServerService ServerSrvice { get { return _serverService; } }
        public IClientService ClientSrvice { get { return _clientService; } }
        public void Dispose()
        {
            _serverService.StopService();
        }
    }
}
