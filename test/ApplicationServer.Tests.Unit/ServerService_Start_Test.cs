using ApplicationShare.Services;
using DomainShare.Enums;
using DomainShare.Models;
using FluentAssertions;
using InfrastructureShare.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Server.Application.Services;
using Tynamix.ObjectFiller;

namespace ApplicationServer.Tests.Unit
{
    public class ServerService_Start_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly IServerService _serverService;
        private readonly Mock<IServerMessageProvider> _moqServerMessageProvider;
        private readonly Mock<IMessageResolver> _moqMessageRsolver;
        public ServerService_Start_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _moqServerMessageProvider = new Mock<IServerMessageProvider>();
            _moqMessageRsolver = new Mock<IMessageResolver>();
            _serverService = new ServerService(_moqServerMessageProvider.Object, _moqMessageRsolver.Object);
        }
        [Fact]
        public async void Start_ListenMessage_ShouldWorkProbably()
        {
            //Arrange
            var onReceive = new TaskCompletionSource<bool>();
            bool messageRecieved = false;
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
                .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract);
                messageRecieved = true;
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            messageRecieved.Should().BeTrue();

        }
        [Fact]
        public async void Start_RegisterClient_ShouldWorkProbably()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };
            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOnline)).Returns(Task.FromResult(true));
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
             .Callback<Func<MessageContract, Task<bool>>>((f) =>
             {
                 f(messageContract);
                 f(messageContract2);
                 onReceive.SetResult(true);
                 result = true;
             });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOnline), Times.AtLeastOnce);

        }
        [Fact]
        public async void Start_RemoveClient_ShouldWorkProbably()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };
            var messageContract3 = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOffline };
            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOffline)).Returns(Task.FromResult(true));
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
            .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract);
                f(messageContract2);
                f(messageContract3);
                onReceive.SetResult(true);
                result = true;
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOffline), Times.AtLeastOnce);
            _moqServerMessageProvider.Verify(mp => mp.RemoveClientSession(It.IsAny<ContactInfo>()), Times.Once);

        }
        [Fact]
        public async void Start_SendMessage_ShouldWorkProbably()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };
            var messageContract3 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, Reciever = new ContactInfo { Id = "Id1" }, Message = "Sample", MessageType = MessageType.Message };
            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.Message)).Returns(Task.FromResult(true));

            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
            .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract);
                f(messageContract2);
                f(messageContract3);
                onReceive.SetResult(true);
                result = true;
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.Message), Times.AtLeastOnce);

        }




        [Fact]
        public async Task Start_RegisterClient_WhenMessageIsNotValid_OperationIsNotValid()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };


            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOnline)).Returns(Task.FromResult(false));
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
            .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract);
                f(messageContract2);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeFalse();

        }
        [Fact]
        public async Task Start_RemoveClient_WhenMessageIsNotValid_OperationIsNotValid()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };
            var messageContract3 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOffline };
            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOffline)).Returns(Task.FromResult(false));
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
            .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract);
                f(messageContract2);
                f(messageContract3);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeFalse();

        }
        [Fact]
        public async Task Start_SendMessageClient_WhenMessageIsNotValid_OperationIsNotValid()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract1 = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, Reciever = new ContactInfo { Id = "Id1" }, Message = string.Empty, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, Reciever = new ContactInfo { Id = "Id1" }, Message = string.Empty, MessageType = MessageType.NotifyOnline };
            var messageContract3 = new MessageContract { Sender = new ContactInfo { Id = "" }, Reciever = new ContactInfo { Id = "Id1" }, Message = string.Empty, MessageType = MessageType.Message };
            _moqServerMessageProvider.Setup(a => a.SendMessageAsync(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.Message)).Returns(Task.FromResult(false));
            _moqMessageRsolver.Setup(a => a.ResolveMessages(It.IsAny<Func<MessageContract, Task<bool>>>()))
            .Callback<Func<MessageContract, Task<bool>>>((f) =>
            {
                f(messageContract1);
                f(messageContract2);
                f(messageContract3);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService();
            await onReceive.Task;
            //Assert
            result.Should().BeFalse();


        }

    }
}