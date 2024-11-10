using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface IMessageResolver
    {
        void StartRecieve(Func<MessageContract, Task<bool>> func);
        void ReadChunkMessage(MessageChunk chunk);
        public long MessageCount { get; }
    }
}
