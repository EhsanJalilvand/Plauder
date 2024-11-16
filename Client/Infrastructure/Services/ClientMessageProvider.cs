using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using System;
using ApplicationShare.Services;
using System.Text.Json;
using System.Net.Sockets;
using Client.Application.Services;

namespace InfrastructureClient.Services
{
    public class ClientMessageProvider : IClientMessageProvider
    {
        private readonly ISocketClientProvider _socketProvider;
        private readonly ServerSetting _serverSetting;
        private readonly IMessageQueueManager _messageQueueManager;
        private readonly IMessageResolver _messageResolver;
        public ClientMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager messageQueueManager, IMessageResolver messageResolver, ISocketClientProvider socketProvider)
        {
            _serverSetting = option.Value;
            _messageQueueManager = messageQueueManager;
            _messageResolver = messageResolver;
            _socketProvider = socketProvider;
        }
        public async void StartService(Action connected)
        {
            while (!_socketProvider.IsConnected)
            {
               await _socketProvider.TryConnect();
               await Task.Delay(1000);
            }
            connected();
            _messageQueueManager.StartSend(async (a) =>
            {
                try
                {
                    await _socketProvider.SendAsync(a);
                }
                catch (Exception)
                {
                    await _socketProvider.ReconnectSocketAsync();
                    return false;
                }
                return true;
            });
        }
        public async Task ReceiveMessageAsync()
        {
            _socketProvider.ReceiveAsync((chunkMessage) =>
            {
                _messageResolver.ReadChunkMessage(chunkMessage);
            });
        }
        public async Task<bool> SendMessage(ContactInfo contact, string message, MessageType messageType)
        {
            var standardMessage = new MessageContract() { Reciever = contact, Message = message, MessageType = messageType };
            _messageQueueManager.PushToQueue(standardMessage);
            return true;
        }
    }
}