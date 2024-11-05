using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Models;
using DomainShare.Settings;
using FluentAssertions;
using InfrastructureClient.Services;
using Microsoft.Extensions.Options;
using Moq;
using Share.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationClient.Tests.Unit
{
    public class ClientService_SendMessage_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly Mock<IClientMessageProvider> _mockMessageProvider;
        private readonly IClientService _clientService;
        private readonly Mock<IMessageResolver> _mockMessageResolver;
        public ClientService_SendMessage_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _mockMessageProvider = new Mock<IClientMessageProvider>();
            _mockMessageResolver = new Mock<IMessageResolver>();
            _clientService = new ClientService(_mockMessageProvider.Object, _dataFixture.ServerSettingOption, _mockMessageResolver.Object);
        }
        [Fact]
        public async Task SendMessage_WorkProbably()
        {
            //Arrange
            var contact = new ContactInfo() { Id = "127.0.0.1:8070", UserName = "userName" };
            string sampleMessage = "This Is My Sample Message";


            //Act
            var result = await _clientService.SendMessage(contact, sampleMessage);
            _mockMessageProvider.Verify(a => a.SendMessage(contact, sampleMessage, DomainShare.Enums.MessageType.Message), Times.Once);

            //Assert
            result.Should().BeTrue();
        }
        [Fact]
        public async Task SendMessage_WhenMessageIsEmpty_ShouldThrowException()
        {
            //Arrange
            var contact = new ContactInfo() { Id = "127.0.0.1:8070", UserName = "userName" };
            string sampleMessage = string.Empty;


            //Act
            var task = async () => await _clientService.SendMessage(contact, sampleMessage);

            //Assert
            await task.Should().ThrowAsync<InvalidOperationException>();
        }
        [Fact]
        public async Task SendMessage_WhenSenderContactIsNotValid_ShouldThrowException()
        {
            //Arrange
            var contact = new ContactInfo() { Id = "", UserName = "userName" };
            string sampleMessage = "Sample Message";


            //Act
            var task = async () => await _clientService.SendMessage(contact, sampleMessage);

            //Assert
            await task.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
