using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface ISocketProvider
    {
        public Socket Socket { get; }
        Task<bool> TryConnect();
        Task ReconnectSocketAsync();
        Task<int> ReceiveAsync(ArraySegment<byte> buffer, SocketFlags socketFlags);
    }
}
