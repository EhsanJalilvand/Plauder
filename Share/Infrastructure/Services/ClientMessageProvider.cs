using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Net;
using System.Text;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using System;
using ApplicationShare.Services;
using System.Text.Json;

namespace InfrastructureShare.Services
{
    public class ClientMessageProvider : IClientMessageProvider
    {
        private readonly ISocketProvider _socketProvider;
        private readonly ServerSetting _serverSetting;
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly IMessageResolver _messageResolver;
        public ClientMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager messageQueueManager, IMessageResolver messageResolver, ISocketProvider socketProvider)
        {
            _serverSetting = option.Value;
            _messageQueueManager = messageQueueManager;
            _messageResolver = messageResolver;
            _socketProvider = socketProvider;
        }
        public void StartService(Action connected)
        {
            while (_socketProvider.Socket == null || !_socketProvider.Socket.Connected)
            {
                _socketProvider.TryConnect();
            }
            connected();
            _messageQueueManager.StartSend(async (a) =>
            {
                try
                {
                    if (!_socketProvider.Socket.Connected)
                        await _socketProvider.ReconnectSocketAsync();
                    var message = JsonSerializer.Serialize(a) + "<EOF>";
                    byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
                    await _socketProvider.Socket.SendAsync(new ArraySegment<byte>(binaryChunk), SocketFlags.None);
                }
                catch (SocketException)
                {
                    await _socketProvider.ReconnectSocketAsync();
                    return false;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions if needed
                    return false;
                }

                return true;


            });
        }
        public async Task ReceiveMessageAsync()
        {

            var buffer = new byte[_serverSetting.ChunkSize];
            var data = new StringBuilder();
            int tryConnectCount = 0;
            while (tryConnectCount<3)
            {
                try
                {
                    int bytesRead = await _socketProvider.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (bytesRead == 0) break;

                    data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    var messageText = data.ToString();
                    if (!messageText.EndsWith("<EOF>"))
                        continue;
                    var messages = messageText.Split("<EOF>");
                    foreach (var message in messages)
                    {
                        if (string.IsNullOrEmpty(message)) continue;
                        var chunkMessage = message.ConvertToObject<MessageChunk>();
                        _messageResolver.ReadChunkMessage(chunkMessage);
                    }
                    data.Clear();
                }
                catch (SocketException)
                {
                    await Task.Delay(1000);
                    tryConnectCount++;
                    await _socketProvider.ReconnectSocketAsync();
                }


            }
        }
        public async Task<bool> SendMessage(ContactInfo contact, string message, MessageType messageType)
        {
            var standardMessage = new MessageContract() { Reciever = contact, Message = message, MessageType = messageType };
            _messageQueueManager.PushToQueue(standardMessage);
            return true;
        }
    }
}