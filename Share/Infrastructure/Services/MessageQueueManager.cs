using ApplicationShare.Services;
using DomainShare.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class MessageQueueManager : IMessageQueueManager
    {
        private readonly ConcurrentQueue<MessageChunk> senderMessageQueues = new ConcurrentQueue<MessageChunk>();
        private readonly IMessageChunker _messageChunker;

        public long MessageChunkCount => senderMessageQueues.Count;

        public MessageQueueManager(IMessageChunker messageChunker)
        {
            _messageChunker = messageChunker;
        }
        public void StartSend(Func<MessageChunk,Task<bool>> func)
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    senderMessageQueues.TryPeek(out var _messageChunk);
                    if(_messageChunk != null)
                    if (await func(_messageChunk))
                        senderMessageQueues.TryDequeue(out var _);
                }
            });
        }
        public void PushToQueue(MessageContract messageContract)
        {
            _messageChunker.ChunckMessage(messageContract, (chunk) =>
            {
                senderMessageQueues.Enqueue(chunk);
            });
        }

    }
}
