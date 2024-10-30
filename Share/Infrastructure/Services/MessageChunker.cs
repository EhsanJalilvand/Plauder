using ApplicationShare.Services;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class MessageChunker : IMessageChunker
    {
        private readonly ServerSetting _serverSetting;
        public MessageChunker(IOptions<ServerSetting> options)
        {
            _serverSetting = options.Value;
        }
        public void ChunckMessage(MessageContract message, Action<MessageChunk> MessageChunkCallBack)
        {
            if (message == null)
                return;
            var messageText = message.ConvertToJson();
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageText);
            var chunkArray = messageBytes.Chunk(_serverSetting.ChunkSize);
            int index = 0;
            Guid messageId = Guid.NewGuid();
            foreach (var data in chunkArray)
            {
                {
                    MessageChunk messageChunk = new MessageChunk();
                    messageChunk.RecieverId = message.Reciever?.Id;
                    messageChunk.Message = data;
                    messageChunk.MessageId = messageId;
                    messageChunk.ChunkNumber = index;
                    messageChunk.TotalChunk = chunkArray.Count();
                    index++;
                    MessageChunkCallBack(messageChunk);
                }
            }
        }
        public MessageContract Assemble(List<MessageChunk> messageChunks)
        {
            if (messageChunks == null || messageChunks.Count == 0)
                return null;
            var messageBytes = messageChunks.OrderBy(chunk => chunk.ChunkNumber)
           .SelectMany(chunk => chunk.Message)
           .ToArray();

            var messageText = Encoding.UTF8.GetString(messageBytes);

            return messageText.ConvertToObject<MessageContract>();
        }

        public bool CanAssemble(List<MessageChunk> messageChunks)
        {
            if (messageChunks == null || !messageChunks.Any())
                return false;
            if (messageChunks.Count() != messageChunks[0].TotalChunk)
                return false;
            return messageChunks
                 .OrderBy(o => o.ChunkNumber)
                 .Select((chunk, index) => chunk.ChunkNumber == index)
                 .All(a => a);
        }
    }
}
