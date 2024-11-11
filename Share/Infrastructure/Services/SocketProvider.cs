using ApplicationShare.Services;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class SocketProvider : ISocketProvider
    {
        private Socket _socket;
        private static readonly object _lock = new object();
        private readonly ServerSetting _serverSetting;
        public SocketProvider(IOptions<ServerSetting> options)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSetting = options.Value;
        }
        public Socket Socket => _socket;
        public async Task<int> ReceiveAsync(ArraySegment<byte> buffer, SocketFlags socketFlags)
        {
            return await _socket.ReceiveAsync(buffer, socketFlags);
        }
        public async Task<bool> TryConnect()
        {
            try
            {
                _socket.Connect(new IPEndPoint(IPAddress.Parse(_serverSetting.Ip), _serverSetting.Port));
            }
            catch (SocketException)
            {
                await Task.Delay(5000);
                await TryConnect();
            }
            return true;
        }

        public async Task ReconnectSocketAsync()
        {
            lock (_lock)
            {
                if (_socket != null && _socket.Connected)
                    return;

                _socket?.Dispose();
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                TryConnect();
            }
        }
    }
}
