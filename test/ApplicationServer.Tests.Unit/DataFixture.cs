using DomainShare.Settings;
using Microsoft.Extensions.Options;
using Share.Tests.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationServer.Tests.Unit
{
    public class DataFixture : IDisposable
    {
        private readonly IOptions<ServerSetting> _serverSettingOptions;
        public DataFixture()
        {
            _serverSettingOptions = ServerSettingBuilder.Build();
        }
        public IOptions<ServerSetting> ServerSettingOption { get { return _serverSettingOptions; } }
        public void Dispose()
        {

        }
    }
}
