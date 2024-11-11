using ApplicationShare.Services;
using DomainShare.Enums;
using DomainShare.Models;
using FluentAssertions;
using InfrastructureShare.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Tests.Unit
{
    public class ClientMessageProvider_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly IClientMessageProvider _clientMessageProvider;
        private readonly Mock<IMessageQueueManager> _moqQueueMessageManager;
        private readonly Mock<IMessageResolver> _moqMessageResolver;
        private readonly Mock<ISocketProvider> _moqSocketProvider;
        public ClientMessageProvider_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _moqQueueMessageManager = new Mock<IMessageQueueManager>();
            _moqMessageResolver = new Mock<IMessageResolver>();
            _moqSocketProvider = new Mock<ISocketProvider>();
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

            _moqSocketProvider.Setup(a => a.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), SocketFlags.None))
                .ReturnsAsync(buffer.Length)
                .Callback((ArraySegment<byte> b, SocketFlags f) =>
                {
                    Array.Copy(buffer, b.Array, buffer.Length);
                    _moqSocketProvider.Setup(a => a.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), SocketFlags.None)).ReturnsAsync(0);
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


            _moqSocketProvider.Setup(a => a.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), SocketFlags.None))
                .ReturnsAsync(buffer.Length)
                .Callback((ArraySegment<byte> b, SocketFlags f) =>
                {
                    throw new SocketException();
                });



            //Act
            await _clientMessageProvider.ReceiveMessageAsync();

            await Task.Delay(4000);

            //Assert
            _moqSocketProvider.Verify(a => a.ReconnectSocketAsync(), Times.Exactly(3));
        }
        [Fact]
        public void StartService_ShouldSendMessageInQueue()
        {

        }
    }
}
