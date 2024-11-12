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
        public bool IsConnected =>_socket!=null && _socket.Connected;
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
        public async Task<int> ReceiveAsync(ArraySegment<byte> buffer, SocketFlags socketFlags)
        {
            return await _socket.ReceiveAsync(buffer, socketFlags);
        }
        public async Task<int> SendAsync(ArraySegment<byte> buffer, SocketFlags socketFlags)
        {
            return await SendAsync(_socket,buffer, socketFlags);
        }
        public async Task<int> SendAsync(Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags)
        {
            return await socket.SendAsync(buffer, socketFlags);
        }
        public Task ListenAsync(Action<Socket, string> clientSocket)
        {
            return Task.Factory.StartNew(() =>
            {
                IPHostEntry iPHost = Dns.GetHostEntry(_serverSetting.Ip);
                IPAddress iPaddress = iPHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(iPaddress, _serverSetting.Port);
                //Socket socket = new Socket(iPaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                Socket.Bind(localEndPoint);
                // Using Listen() method we create 
                // the Client list that will want
                // to connect to Server
                Socket.Listen(10);
                while (true)
                {
                    // Suspend while waiting for
                    // incoming connection Using 
                    // Accept() method the server 
                    // will accept connection of client
                    Socket acceptedSocket = Socket.Accept();
                    IPEndPoint remoteEndPoint = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    string clientIP = remoteEndPoint?.Address.ToString();
                    int clientPort = remoteEndPoint?.Port ?? 0;
                    string clientId = $"{clientIP}:{clientPort}";
                    clientSocket(acceptedSocket, clientId);
                }
            });
        }


    }
}
