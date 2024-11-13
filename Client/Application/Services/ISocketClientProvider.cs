using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Application.Services
{
    public interface ISocketClientProvider
    {
        public bool IsConnected { get; }
        Task<bool> TryConnect();
        Task ReconnectSocketAsync();
        void ReceiveAsync(Action<MessageChunk> messageChunk);
        Task<int> SendAsync(MessageChunk messageChunk);
    }
}
