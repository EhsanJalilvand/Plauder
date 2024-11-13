using DomainShare.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Application.Tests.Integration
{
    public class ServerMessageTransfer_Test:IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        public ServerMessageTransfer_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            Task.Factory.StartNew(() =>
            {
                _dataFixture.ServerSrvice.StartService((a) => { });
            });
        }
        [Fact]
        public async void MessageShouldSendSuccessfully()
        {
            bool isClientToServerConnected = false;

            _dataFixture.ClientSrvice.Start(() =>
            {
                isClientToServerConnected=true;
            }, (message) =>
            {
                var sss = message;
            });

           await Task.Delay(TimeSpan.FromSeconds(3));

            isClientToServerConnected.Should().BeTrue();
        }
    }
}