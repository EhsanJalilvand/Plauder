using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Reflection;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using ApplicationShare.Services;
using InfrastructureShare.Services;
using System.Text.Json;

namespace InfrastructureShare.Services

{
    public class ServerMessageProvider : IServerMessageProvider
    {
        private readonly ServerSetting _serverSetting;
        private readonly IMessageQueueManager _queueManager;
        private readonly IMessageResolver _messageResolver;
        private readonly ISocketProvider _socketProvider;
        private readonly ISocketManager _socketManager;
        public ServerMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager queueManager, IMessageResolver messageResolver, ISocketProvider socketProvider, ISocketManager socketManager)
        {
            _serverSetting = option.Value;
            _queueManager = queueManager;
            _messageResolver = messageResolver;
            _socketProvider = socketProvider;
            _socketManager = socketManager;
        }
        public void SendQueueMessagesToClients()
        {
            _queueManager.StartSend(async (a) =>
            {
                try
                {
                    if (!_socketManager.TrySocket(a.RecieverId))
                        return false;
                    var message = JsonSerializer.Serialize(a) + "<EOF>";
                    byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
                    var socket = _socketManager.FindSocket(a.RecieverId);
                    await _socketProvider.SendAsync(socket, new ArraySegment<byte>(binaryChunk), SocketFlags.None);
                }
                catch (SocketException)
                {
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
        public Task ListenMessageAsync()
        {
            _socketProvider.ListenAsync((clientSocket, clientId) =>
            {

               if(_socketManager.AddSocket(clientSocket, clientId));
                Task.Run(() => HandleClient(clientSocket, clientId));
            });
            return Task.CompletedTask;
        }
        private async Task HandleClient(Socket client, string clientId)
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
                            _messageResolver.ReadChunkMessage(chunkMessage);
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
        public async Task<bool> RemoveClientSession(ContactInfo sender)
        {
            return _socketManager.RemoveSocket(sender.Id);
        }
        public async Task<bool> SendMessageAsync(ContactInfo sender, ContactInfo receiver, string message, MessageType messageType)
        {
            if (!_socketManager.IsExist(receiver.Id))
            {
                return false;
            }
            var standardMessage = new MessageContract { Sender = sender, Reciever = receiver, Message = message, MessageType = messageType };
            _queueManager.PushToQueue(standardMessage);
            return true;
        }


    }
}
