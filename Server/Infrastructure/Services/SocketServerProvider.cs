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
    public class SocketServerProvider : ISocketServerProvider
    {
        private static readonly object _lock = new object();
        private readonly ServerSetting _serverSetting;
        private readonly ISocketManager _socketManager;
        private Socket _socket;
        public SocketServerProvider(IOptions<ServerSetting> options, ISocketManager socketManager)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSetting = options.Value;
            _socketManager = socketManager;
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
                }
            }
        }
        public async Task<int> SendAsync(string recieverId, MessageChunk messageChunk)
        {
            var socket= _socketManager.FindSocket(recieverId);
            var message = JsonSerializer.Serialize(messageChunk) + "<EOF>";
            byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
            return await socket.SendAsync(binaryChunk, SocketFlags.None);
        }
        public Task ListenAsync(Action<MessageChunk> messageChunk)
        {
            return Task.Factory.StartNew(() =>
            {
                IPHostEntry iPHost = Dns.GetHostEntry(_serverSetting.Ip);
                IPAddress iPaddress = iPHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(iPaddress, _serverSetting.Port);
                //Socket socket = new Socket(iPaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                _socket.Bind(localEndPoint);
                // Using Listen() method we create 
                // the Client list that will want
                // to connect to Server
                _socket.Listen(10);
                while (true)
                {
                    // Suspend while waiting for
                    // incoming connection Using 
                    // Accept() method the server 
                    // will accept connection of client
                    Socket acceptedSocket = _socket.Accept();
                    IPEndPoint remoteEndPoint = acceptedSocket.RemoteEndPoint as IPEndPoint;
                    string clientIP = remoteEndPoint?.Address.ToString();
                    int clientPort = remoteEndPoint?.Port ?? 0;
                    string clientId = $"{clientIP}:{clientPort}";
                    if (_socketManager.AddSocket(acceptedSocket, clientId));
                    HandleClient(acceptedSocket, clientId, messageChunk);
                }
            });
        }
        private async Task HandleClient(Socket client, string clientId, Action<MessageChunk> messageChunk)
        {
            byte[] buffer = new byte[_serverSetting.ChunkSize];
            StringBuilder data = new StringBuilder();
            try
            {
                while (_socketManager.IsExist(clientId))
                {
                    try
                    {
                        int numByte = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                        if (numByte == 0) break;

                        data.Append(Encoding.UTF8.GetString(buffer, 0, numByte));
                        var messageText = data.ToString();
                        if (!messageText.EndsWith("<EOF>"))
                            continue;
                        var messages = messageText.Split("<EOF>");
                        foreach (var message in messages)
                        {
                            if (string.IsNullOrEmpty(message)) continue;
                            var chunkMessage = message.ConvertToObject<MessageChunk>();
                            chunkMessage.ClientId = clientId;
                            messageChunk(chunkMessage);
                        }
                        data.Clear();
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
                client.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _socketManager.RemoveSocket(clientId);
            }
        }

    }
}
