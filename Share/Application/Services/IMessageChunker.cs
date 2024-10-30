using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface IMessageChunker
    {
        void ChunckMessage(MessageContract message,Action<MessageChunk> MessageChunkCallBack);
        bool CanAssemble(List<MessageChunk> messageChunks);
        MessageContract Assemble(List<MessageChunk> messageChunks);
    }
}
