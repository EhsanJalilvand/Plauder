using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Enums;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureClient.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientMessageProvider _messageProvider;
        private readonly IMessageResolver _messageResolver;
        public ClientService(IClientMessageProvider messageProvider, IOptions<ServerSetting> options, IMessageResolver messageResolver)
        {
            _messageProvider = messageProvider;
            _messageResolver = messageResolver;
        }

        public void Start(Action connected, Action<MessageContract> messageCallback)
        {
            Task.Factory.StartNew(() => InitializeConnection(connected, messageCallback));
        }

        private void InitializeConnection(Action connected, Action<MessageContract> messageCallback)
        {
            _messageProvider.StartService(async () =>
            {
                connected();
                StartReceivingMessages(messageCallback);
                await _messageProvider.ReceiveMessageAsync();
            });
        }

        private void StartReceivingMessages(Action<MessageContract> messageCallback)
        {
            _messageResolver.ResolveMessages(async (MessageContract a) =>
            {
                messageCallback(a);
                return true;
            });
        }

        public async Task<bool> UnRegisterClient()
        {
            await _messageProvider.SendMessage(null, String.Empty, MessageType.NotifyOffline);
            return true;
        }
        public async Task<bool> RegisterClient(ContactInfo contactInfo)
        {
            if (contactInfo == null || string.IsNullOrEmpty(contactInfo.UserName))
                throw new InvalidOperationException("Contact Is Not Valid");
            await _messageProvider.SendMessage(contactInfo, contactInfo.UserName, MessageType.NotifyOnline);
            return true;
        }
        public async Task<bool> SendMessage(ContactInfo contactInfo, string message)
        {
            if (contactInfo == null || string.IsNullOrEmpty(contactInfo.Id))
                throw new InvalidOperationException("Contact Is Not Valid");
            if (string.IsNullOrEmpty(message))
                throw new InvalidOperationException("Message Is Null");
            await _messageProvider.SendMessage(contactInfo, message, MessageType.Message);
            return true;
        }

    }
}
