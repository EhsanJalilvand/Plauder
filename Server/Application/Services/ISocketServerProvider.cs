using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface ISocketServerProvider
    {
        void ReceiveAsync(Action<MessageChunk> messageChunk);
        Task<int> SendAsync(string recieverId , MessageChunk messageChunk);
        Task ListenAsync(Action<MessageChunk> messageChunk);
    }
}
