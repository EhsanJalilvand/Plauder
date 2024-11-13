using ApplicationShare.Services;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class SocketClientProvider : ISocketClientProvider
    {
        private Socket _socket;
        private static readonly object _lock = new object();
        private readonly ServerSetting _serverSetting;
        public SocketClientProvider(IOptions<ServerSetting> options)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSetting = options.Value;
        }
        public Socket Socket => _socket;
        public bool IsConnected => _socket != null && _socket.Connected;
        public async Task<bool> TryConnect()
        {
            try
            {
                _socket.Connect(new IPEndPoint(IPAddress.Parse(_serverSetting.Ip), _serverSetting.Port));
            }
            catch (SocketException ex)
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
        public async void ReceiveAsync(Action<MessageChunk> messageChunk)
        {
            var buffer = new byte[_serverSetting.ChunkSize];
            var data = new StringBuilder();
            int tryConnectCount = 0;
            while (tryConnectCount < 3)
            {
                try
                {
                    int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesRead == 0) break;

                    data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    var messageText = data.ToString();
                    if (!messageText.EndsWith("<EOF>"))
                        continue;
                    var messages = messageText.Split("<EOF>");
                    foreach (var m in messages)
                    {
                        if (string.IsNullOrEmpty(m)) continue;
                        var chunkMessage = m.ConvertToObject<MessageChunk>();
                        messageChunk(chunkMessage);
                    }
                    data.Clear();
                }
                catch (SocketException)
                {
                    await Task.Delay(1000);
                    tryConnectCount++;
                    await ReconnectSocketAsync();
                }
            }
        }
        public async Task<int> SendAsync(MessageChunk messageChunk)
        {
            if (!IsConnected)
                await ReconnectSocketAsync();
            var message = JsonSerializer.Serialize(messageChunk) + "<EOF>";
            byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
            return await _socket.SendAsync(binaryChunk, SocketFlags.None);
        }

    }
}
