using ApplicationShare.Services;
using DomainShare.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class MessageResolver : IMessageResolver
    {
        private readonly IMessageChunker _messageChunker;
        private readonly ConcurrentDictionary<Guid, List<MessageChunk>> _chunkMessages = new ConcurrentDictionary<Guid, List<MessageChunk>>();
        public MessageResolver(IMessageChunker messageChunker)
        {
            _messageChunker = messageChunker;
        }
        public void ReadChunkMessage(MessageChunk chunk)
        {
            _chunkMessages.AddOrUpdate(chunk.MessageId,
                _ => new List<MessageChunk> { chunk },// If the key is not present, add a new list with the item
                (_, existingList) =>
                {
                    existingList.Add(chunk);// If the key exists, add the item to the existing list
                    return existingList;
                });

        }

        public void StartRecieve(Func<MessageContract, Task<bool>> func)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var keysToRemove = new List<Guid>();
                    foreach (var chunkList in _chunkMessages.Values)
                    {
                        if (_messageChunker.CanAssemble(chunkList))
                        {
                            var message = _messageChunker.Assemble(chunkList);
                            if (message != null)
                            {
                                message.Sender = message.Sender ?? new ContactInfo() { Id = chunkList[0].ClientId };
                                if (await func(message))
                                    keysToRemove.Add(chunkList[0].MessageId);
                            }
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        _chunkMessages.TryRemove(key, out _);
                    }
                }

            });
        }
    }
}
