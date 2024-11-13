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
using Xunit;

namespace ApplicationClient.Tests.Unit
{
    public class ClientService_RegisterClient_Test: IClassFixture<DataFixture>
    {
        Mock<IClientMessageProvider> _mockMessageProvider;
        Mock<IMessageResolver> _messageResolver;
        private readonly IClientService _clientService;
        private readonly DataFixture _dataFixture;
        public ClientService_RegisterClient_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _mockMessageProvider = new Mock<IClientMessageProvider>();
            _messageResolver = new Mock<IMessageResolver>();
            _clientService = new ClientService(_mockMessageProvider.Object, _dataFixture.ServerSettingOption, _messageResolver.Object);
        }
        [Fact]
       public async Task RegisterClient_ValidContactInfo_ReturnsTrue()
        {
            //Arrnage
            var contact = new DomainShare.Models.ContactInfo() { UserName = "MyName", Id = "127.0.0.1:90435" };
            _mockMessageProvider.Setup(a => a.SendMessage(contact, string.Empty, DomainShare.Enums.MessageType.NotifyOnline)).ReturnsAsync(() => true);
            //Act
            var result = await _clientService.RegisterClient(contact);

            //Assert
            result.Should().BeTrue();
            _mockMessageProvider.Verify(a=>a.SendMessage(contact,It.IsAny<string>(),DomainShare.Enums.MessageType.NotifyOnline), Times.Once());
        }
        [Fact]
        public async Task RegisterClient_InValidContactInfo_RasieException()
        {
            //Arrnage
            var contact = new DomainShare.Models.ContactInfo() { UserName = string.Empty, Id = "127.0.0.1:90435" };
            _mockMessageProvider.Setup(a => a.SendMessage(contact, string.Empty, DomainShare.Enums.MessageType.NotifyOnline)).ReturnsAsync(() => true);
            //Act
            var act = async () => { return await _clientService.RegisterClient(contact); };

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
