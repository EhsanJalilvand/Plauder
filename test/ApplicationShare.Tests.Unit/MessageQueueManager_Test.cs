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
    public class MessageQueueManager_Test
    {
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly Mock<IMessageChunker> _moqMessageChunker;
        public MessageQueueManager_Test()
        {
            _moqMessageChunker = new Mock<IMessageChunker>();
            _messageQueueManager = new MessageQueueManager(_moqMessageChunker.Object);
        }
        [Fact]
        public void PushToQueue_ShouldEnqueueMessageChunks()
        {
            //Arrange
            var beforePush_MessageCount = _messageQueueManager.MessageChunkCount;
            var messageId = Guid.NewGuid();
            var messageContract = new MessageContract() { Message = "Hello, this is a test message!", Reciever = new ContactInfo() { Id = "Id" } };
            var messageChunk = new MessageChunk() { ChunkNumber = 0, ClientId = "Id1", Message = Encoding.UTF8.GetBytes("Test"), MessageId = messageId };
            _moqMessageChunker.Setup(a => a.ChunckMessage(It.IsAny<MessageContract>(), It.IsAny<Action<MessageChunk>>())).
                Callback<MessageContract, Action<MessageChunk>>((contract, callback) =>
                {
                    var messageChunks = new List<MessageChunk> {
                        new MessageChunk { MessageId = Guid.NewGuid(), ChunkNumber = 0, Message = Encoding.UTF8.GetBytes("Hello, ") },
                        new MessageChunk { MessageId = Guid.NewGuid(), ChunkNumber = 1, Message = Encoding.UTF8.GetBytes("this is ") },
                        new MessageChunk { MessageId = Guid.NewGuid(), ChunkNumber = 2, Message = Encoding.UTF8.GetBytes("a test ") },
                        new MessageChunk { MessageId = Guid.NewGuid(), ChunkNumber = 3, Message = Encoding.UTF8.GetBytes("message!") } };
                    foreach (var chunk in messageChunks)
                    {
                        callback(chunk);
                    }
                });

            //Act
            _messageQueueManager.PushToQueue(messageContract);
            var afterPush_MessageCount = _messageQueueManager.MessageChunkCount;
            //Assert
            beforePush_MessageCount.Should().Be(0);
            afterPush_MessageCount.Should().Be(4);

        }

        [Fact]
        public async void StartSend_ShouldDequeueMessageChunks()
        {
            //Arrang
            bool onSendCalled = false;
            var messageChunk = new MessageChunk { MessageId = Guid.NewGuid(), ChunkNumber = 0, TotalChunk = 1, ClientId = "123", Message = Encoding.UTF8.GetBytes("Test message chunk") };
            _moqMessageChunker.Setup(mc => mc.ChunckMessage(It.IsAny<MessageContract>(), It.IsAny<Action<MessageChunk>>()))
                .Callback<MessageContract, Action<MessageChunk>>((msg, callback) =>
                {
                    callback(messageChunk);
                });
            var messageContract = new MessageContract
            {
                Reciever = new ContactInfo { Id = "456" },
                Sender = new ContactInfo { Id = "123" },
                Message = "Hello, this is a test message!"
            };
            _messageQueueManager.PushToQueue(messageContract);
            //Act
            _messageQueueManager.StartSend(async (messageChunk) =>
            {
                onSendCalled = true;
                return true;
            });
            await Task.Delay(100); // Adding delay to ensure the async process is completed
            //Assert
            onSendCalled.Should().BeTrue();
            _messageQueueManager.MessageChunkCount.Should().Be(0);  
            }

    }
    }
