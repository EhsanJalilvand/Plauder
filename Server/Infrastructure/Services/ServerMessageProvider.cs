using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
        private readonly ISocketServerProvider _socketProvider;
        private readonly ISocketManager _socketManager;
        public ServerMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager queueManager, IMessageResolver messageResolver, ISocketServerProvider socketProvider, ISocketManager socketManager)
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
                    await _socketProvider.SendAsync(a.RecieverId,a);
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
            _socketProvider.ListenAsync((chunkMessage) =>
            {
                _messageResolver.ReadChunkMessage(chunkMessage);
            });
            return Task.CompletedTask;
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
