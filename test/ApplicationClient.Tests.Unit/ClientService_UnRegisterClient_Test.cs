using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Models;
using DomainShare.Settings;
using FluentAssertions;
using InfrastructureClient.Services;
using InfrastructureShare.Services;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationClient.Tests.Unit
{
    public class ClientService_UnRegisterClient_Test:IClassFixture<DataFixture>
    {
        Mock<IMessageResolver> _messageResolver;
        private readonly DataFixture _dataFixture;
        Mock<IClientMessageProvider> _mockMessageProvider;
        private readonly IClientService _clientService;
        public ClientService_UnRegisterClient_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _mockMessageProvider = new Mock<IClientMessageProvider>();
            _messageResolver = new Mock<IMessageResolver>();
            _clientService = new ClientService(_mockMessageProvider.Object, _dataFixture.ServerSettingOption, _messageResolver.Object);
        }
        [Fact]
       public async Task UnRegisterClient_ValidContactInfo_ReturnsTrue()
        {
            //Arrnage
            _mockMessageProvider.Setup(a => a.SendMessage(null, string.Empty, DomainShare.Enums.MessageType.NotifyOffline)).ReturnsAsync(() => true);
            //Act
            var result = await _clientService.UnRegisterClient();

            //Assert
            result.Should().BeTrue();
            _mockMessageProvider.Verify(a=>a.SendMessage(null,It.IsAny<string>(),DomainShare.Enums.MessageType.NotifyOffline), Times.Once());
        }
    }
}
