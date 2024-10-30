using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface IMessageQueueManager
    {
        void StartSend(Func<MessageChunk, Task<bool>> func);
        void PushToQueue(MessageContract messageContract);
    }
}
