using ApplicationShare.Services;
using DomainShare.Models;
using FluentAssertions;
using InfrastructureShare.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Tests.Unit
{
    public class MessageResolver_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private readonly IMessageResolver _messageResolver;
        private readonly Mock<IMessageChunker> _moqMessageChunker;
        public MessageResolver_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _moqMessageChunker = new Mock<IMessageChunker>();
            _messageResolver = new MessageResolver(_moqMessageChunker.Object);
        }
        [Fact]
        public void ReadChunkMessage_ShouldAddOrUpdateChunkQueue()
        {
            //Arrange
            var messageId = Guid.NewGuid();
            MessageChunk messageChunk = new MessageChunk() { ChunkNumber = 0, ClientId = "Id0", RecieverId = "Id1", MessageId = messageId, Message = Encoding.UTF8.GetBytes("Hello") };
            MessageChunk messageChunk2 = new MessageChunk() { ChunkNumber = 1, ClientId = "Id0", RecieverId = "Id1", MessageId = messageId, Message = Encoding.UTF8.GetBytes("World") };

            //Act && Assert (First)
            _messageResolver.ReadChunkMessage(messageChunk);
            _messageResolver.MessageCount.Should().Be(1);
            //Act & Assert (Second)
            _messageResolver.ReadChunkMessage(messageChunk2);
            _messageResolver.MessageCount.Should().Be(1);
        }
        [Fact]
        public async void StartRecieve_ShouldProcessChunkAndAssembleMessage()
        {
            //Arrane
            bool isMessageProcced = false;
            var messageId= Guid.NewGuid();
            var messageContract = new MessageContract() { Message = messageId.ToString(), Reciever = new ContactInfo() { Id = "Id1" } };
            _moqMessageChunker.Setup(a => a.CanAssemble(It.IsAny<List<MessageChunk>>())).Returns(true);
            _moqMessageChunker.Setup(a => a.Assemble(It.IsAny<List<MessageChunk>>())).Returns(messageContract);
            //Act
            var chunk = new MessageChunk() { ChunkNumber = 0,MessageId=messageId,Message=Encoding.UTF8.GetBytes("This Is a Test") };
            _messageResolver.StartRecieve(async (contract) =>
            {
                isMessageProcced = contract.Message == messageContract.Message;
                return true;
            });
            _messageResolver.ReadChunkMessage(chunk);
            await Task.Delay(100);
            //Assert
            isMessageProcced.Should().BeTrue();
            _moqMessageChunker.Verify(a => a.CanAssemble(It.IsAny<List<MessageChunk>>()), Times.Once);
            _moqMessageChunker.Verify(a => a.Assemble(It.IsAny<List<MessageChunk>>()), Times.Once);
            _messageResolver.MessageCount.Should().Be(0);
        }
        [Fact]
        public async void StartRecieve_ShouldNotProcessChunk_WhenChunkCanNotAssemble()
        {
            //Arrane
            bool isMessageProcced = false;
            var messageId = Guid.NewGuid();
            var messageContract = new MessageContract() { Message = messageId.ToString(), Reciever = new ContactInfo() { Id = "Id1" } };
            _moqMessageChunker.Setup(a => a.CanAssemble(It.IsAny<List<MessageChunk>>())).Returns(false);
            _moqMessageChunker.Setup(a => a.Assemble(It.IsAny<List<MessageChunk>>())).Returns(messageContract);
            //Act
            var chunk = new MessageChunk() { ChunkNumber = 0, MessageId = messageId, Message = Encoding.UTF8.GetBytes("This Is a Test") };
            _messageResolver.StartRecieve(async (contract) =>
            {
                isMessageProcced = contract.Message == messageContract.Message;
                return true;
            });
            _messageResolver.ReadChunkMessage(chunk);
            await Task.Delay(100);
            //Assert
            isMessageProcced.Should().BeFalse();
            _moqMessageChunker.Verify(a => a.CanAssemble(It.IsAny<List<MessageChunk>>()), Times.AtLeastOnce);
            _moqMessageChunker.Verify(a => a.Assemble(It.IsAny<List<MessageChunk>>()), Times.Never);
            _messageResolver.MessageCount.Should().Be(1);
        }
    }
}
