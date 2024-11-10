using ApplicationShare.Services;
using DomainShare.Models;
using InfrastructureShare.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
namespace ApplicationShare.Tests.Unit
{
    public class MessageChunker_Test : IClassFixture<DataFixture>
    {
        private readonly IMessageChunker _messageChunker;
        private readonly DataFixture _dataFixture;
        public MessageChunker_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _messageChunker = new MessageChunker(dataFixture.ServerSettingOption);
        }
        [Fact]
        public void ChunkMessage_ShuldChunkMessageCorrectly()
        {
            //Arrange
            var messageContract = new MessageContract() { Reciever = new ContactInfo() { Id = "Id123" }, Message = "This Is Test", MessageType = DomainShare.Enums.MessageType.Message };
            List<MessageChunk> messageChunks = new List<MessageChunk>();
            var messageJson = messageContract.ConvertToJson();
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var chunkNumber = messageBytes.Length / _dataFixture.ServerSettingOption.Value.ChunkSize;
            //Act
            _messageChunker.ChunckMessage(messageContract, (a) => { messageChunks.Add(a); });

            bool isOrdered = true;
            var ordered = messageChunks.OrderBy(o => o.ChunkNumber).ToList();
            for (int i = 0; i < messageChunks.Count; i++)
            {
                if (ordered[i].ChunkNumber != i)
                {
                    isOrdered = false;
                    break;
                }

            }
            //Assert
            chunkNumber.Should().Be(chunkNumber);
            isOrdered.Should().BeTrue();
        }
        [Fact]
        public void Assemble_ShouldWorkProbably()
        {
            //Arrange
            var body = "This Is a Test";
            var messageContract = new MessageContract() { Message = body };
            var myMessage = messageContract.ConvertToJson();
            var chunks = new List<MessageChunk>();
            for (int i = 0; i < myMessage.Length; i++)
            {
                chunks.Add(new MessageChunk() { ChunkNumber = i, Message = Encoding.UTF8.GetBytes(myMessage[i].ToString()) });
            }

            //Act
            var result = _messageChunker.Assemble(chunks);
            //Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(body);

        }
        [Fact]
        public void CanAssemble_ShouldReturnTrue_WhenChunksAreComplete()
        {
            //Arrange
            var chunks = new List<MessageChunk>()
            {
                new MessageChunk() { ChunkNumber = 0, Message = Encoding.UTF8.GetBytes("This "), TotalChunk = 4 },
                new MessageChunk() { ChunkNumber = 1, Message = Encoding.UTF8.GetBytes("Is "), TotalChunk = 4 },
                new MessageChunk() { ChunkNumber = 2, Message = Encoding.UTF8.GetBytes("a "), TotalChunk = 4 },
                new MessageChunk() { ChunkNumber = 3, Message = Encoding.UTF8.GetBytes("Test"), TotalChunk = 4 },
            };

            //Act
            var result = _messageChunker.CanAssemble(chunks);
            //Assert
            result.Should().BeTrue();
        }
        [Fact]
        public void CanAssemble_ShouldReturnFalse_WhenChunksAreIncomplete()
        {
            //Arrange
            var chunks = new List<MessageChunk>()
            {
                new MessageChunk() { ChunkNumber = 0, Message = Encoding.UTF8.GetBytes("This "), TotalChunk = 4 },
                new MessageChunk() { ChunkNumber = 1, Message = Encoding.UTF8.GetBytes("Is "), TotalChunk = 4 },
                new MessageChunk() { ChunkNumber = 3, Message = Encoding.UTF8.GetBytes("Test"), TotalChunk = 4 },
            };

            //Act
            var result = _messageChunker.CanAssemble(chunks);
            //Assert
            result.Should().BeFalse();
        }
    }

}
