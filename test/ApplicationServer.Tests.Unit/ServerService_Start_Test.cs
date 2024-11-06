using ApplicationClient.Tests.Unit;
using DomainShare.Enums;
using DomainShare.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Server.Application.Services;
using Server.Infrastructure.Services;
using Share.Application.Services;
using Tynamix.ObjectFiller;

namespace ApplicationServer.Tests.Unit
{
    public class ServerService_Start_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly IServerService _serverService;
        private readonly Mock<IServerMessageProvider> _moqServerMessageProvider;
        public ServerService_Start_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _moqServerMessageProvider = new Mock<IServerMessageProvider>();
            _serverService = new ServerService(_moqServerMessageProvider.Object, dataFixture.ServerSettingOption);
        }
        [Fact]
        public async void Start_ListenMessage_ShouldWorkProbably()
        {
            //Arrange
            var onReceive = new TaskCompletionSource<bool>();
            bool messageRecieved = false;
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback(() =>
            {
                messageRecieved = true;
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => {  });
            await onReceive.Task;
            //Assert
            _moqServerMessageProvider.Verify(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>()), Times.Once);

        }
        [Fact]
        public async void Start_RegisterClient_ShouldWorkProbably()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, MessageType = MessageType.NotifyOnline };
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                message(messageContract2);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => { result = a; });
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessage(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOnline), Times.AtLeastOnce);

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
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                message(messageContract2);
                message(messageContract3);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => { result = a; });
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessage(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.NotifyOffline), Times.AtLeastOnce);
            _moqServerMessageProvider.Verify(mp => mp.RemoveClientAsync(It.IsAny<ContactInfo>()), Times.Once);

        }
        [Fact]
        public async void Start_SendMessage_ShouldWorkProbably()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id1" }, MessageType = MessageType.NotifyOnline };
            var messageContract2 = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, Reciever = new ContactInfo { Id = "Id1" }, Message = "Sample", MessageType = MessageType.Message };
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                message(messageContract2);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => { result = a; });
            await onReceive.Task;
            //Assert
            result.Should().BeTrue();
            _moqServerMessageProvider.Verify(mp => mp.SendMessage(It.IsAny<ContactInfo>(), It.IsAny<ContactInfo>(), It.IsAny<string>(), MessageType.Message), Times.AtLeastOnce);

        }




        [Fact]
        public async Task Start_RegisterClient_WhenMessageIsNotValid_OperationIsNotValid()
        {
            //Arrange
            bool result = false;
            var onReceive = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = string.Empty }, MessageType = MessageType.NotifyOnline };
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) =>
            {
                result = a;
            });
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
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = string.Empty }, MessageType = MessageType.NotifyOffline };
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => { result = a; });
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
            var messageContract = new MessageContract { Sender = new ContactInfo { Id = "Id2" }, Reciever = new ContactInfo { Id = "Id1" }, Message = string.Empty, MessageType = MessageType.Message };
            _moqServerMessageProvider.Setup(a => a.ListenMessageAsync(It.IsAny<Action<MessageContract>>())).Callback<Action<MessageContract>>((message) =>
            {
                message(messageContract);
                onReceive.SetResult(true);
            });

            //Act
            _serverService.StartService((a) => { result = a; });
            await onReceive.Task;
            //Assert
            result.Should().BeFalse();


        }

    }
}