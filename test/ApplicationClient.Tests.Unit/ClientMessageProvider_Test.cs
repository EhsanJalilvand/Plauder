using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Enums;
using DomainShare.Models;
using FluentAssertions;
using InfrastructureClient.Services;
using InfrastructureShare.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationClient.Tests.Unit
{
    public class ClientMessageProvider_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly IClientMessageProvider _clientMessageProvider;
        private readonly Mock<IMessageQueueManager> _moqQueueMessageManager;
        private readonly Mock<IMessageResolver> _moqMessageResolver;
        private readonly Mock<ISocketClientProvider> _moqSocketProvider;
        public ClientMessageProvider_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _moqQueueMessageManager = new Mock<IMessageQueueManager>();
            _moqMessageResolver = new Mock<IMessageResolver>();
            _moqSocketProvider = new Mock<ISocketClientProvider>();
            _clientMessageProvider = new ClientMessageProvider(_dataFixture.ServerSettingOption, _moqQueueMessageManager.Object, _moqMessageResolver.Object, _moqSocketProvider.Object);
        }
        [Fact]
        public async void SendMessage_ShouldPushToQueue()
        {
            //Arrange
            var contact = new ContactInfo { Id = "123" };
            var message = "Hello, World!";
            var messageType = MessageType.Message;


            //Act
            var result = await _clientMessageProvider.SendMessage(contact, message, messageType);


            //Assert
            result.Should().BeTrue();
            _moqQueueMessageManager.Verify(a => a.PushToQueue(It.Is<MessageContract>(a =>
            a.Message == message &&
            a.MessageType == messageType &&
            a.Reciever == contact
            )),
            Times.Once);
        }
        [Fact]
        public void ReceiveMessage_ShouldProcessRecievedMessages()
        {
            //Arrange
            var messageChunk = new MessageChunk() { ChunkNumber = 0, ClientId = "Id1", Message = Encoding.UTF8.GetBytes("Test"), MessageId = Guid.NewGuid() };
            var messageChunkJson = messageChunk.ConvertToJson() + "<EOF>";
            var buffer = Encoding.UTF8.GetBytes(messageChunkJson);

            _moqSocketProvider.Setup(a => a.ReceiveAsync(It.IsAny<Action<MessageChunk>>()))
                .Callback<Action<MessageChunk>>((a) =>
                {

                });



            //Act
            _clientMessageProvider.ReceiveMessageAsync();



            //Assert
            _moqMessageResolver.Verify(a => a.ReadChunkMessage(It.Is<MessageChunk>(mc => mc.MessageId == messageChunk.MessageId)), Times.Once);
        }
        [Fact]
        public async void ReceiveMessage_Should3TimeReconnectWhenThrowException()
        {
            //Arrange
            var messageChunk = new MessageChunk() { ChunkNumber = 0, ClientId = "Id1", Message = Encoding.UTF8.GetBytes("Test"), MessageId = Guid.NewGuid() };
            var messageChunkJson = messageChunk.ConvertToJson() + "<EOF>";
            var buffer = Encoding.UTF8.GetBytes(messageChunkJson);


            _moqSocketProvider.Setup(a => a.ReceiveAsync(It.IsAny<Action<MessageChunk>>()))
                .Callback<Action<MessageChunk>>((a) =>
                {

                });



            //Act
            await _clientMessageProvider.ReceiveMessageAsync();

            await Task.Delay(4000);

            //Assert
            _moqSocketProvider.Verify(a => a.ReconnectSocketAsync(), Times.Exactly(3));
        }
        [Fact]
        public async void StartService_ShouldSendQueueMessages_WhenSocketIsConnect()
        {
            //Arrang
            bool isConnected = false;
            var messageCallbackInvoked = new TaskCompletionSource<bool>();
            var messageChunk = new MessageChunk() {ChunkNumber=0,ClientId="Id01",Message=Encoding.UTF8.GetBytes("Test"),MessageId=Guid.NewGuid() };
            var messageChunk2 = new MessageChunk() { ChunkNumber = 0, ClientId = "Id02", Message = Encoding.UTF8.GetBytes("Test"), MessageId = Guid.NewGuid() };
            _moqSocketProvider.Setup(a=>a.IsConnected).Returns(true);
            _moqQueueMessageManager.Setup(a => a.StartSend(It.IsAny<Func<MessageChunk, Task<bool>>>()))
                .Callback<Func<MessageChunk, Task<bool>>>(async (f) =>
            {
                await f(messageChunk);
                await f(messageChunk2);
                messageCallbackInvoked.SetResult(true);
            });
            //Act
            _clientMessageProvider.StartService(() =>
            {
                isConnected = true;
            });
            await messageCallbackInvoked.Task;
            //Assert
            isConnected.Should().BeTrue();
            _moqSocketProvider.Verify(a => a.SendAsync(It.IsAny<MessageChunk>()), Times.Exactly(2));
        }

    }
}
