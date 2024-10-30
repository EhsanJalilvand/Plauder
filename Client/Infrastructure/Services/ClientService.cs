using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Enums;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Options;
using Share.Application.Services;
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
            Task.Factory.StartNew(() =>
            {
                _messageProvider.Initialize(async () =>
                {
                    connected();
                    _messageResolver.StartRecieve(async(a)=> 
                    {
                        messageCallback(a);
                        return true;
                    });
                    await _messageProvider.ReceiveMessageAsync();
                });
            });
        }

        public async Task<bool> CloseSession()
        {
            await _messageProvider.SendMessage(null, String.Empty, MessageType.NotifyOffline);
            return true;
        }
        public async Task<bool> RegisterClient(ContactInfo contactInfo)
        {
            await _messageProvider.SendMessage(contactInfo, contactInfo.UserName, MessageType.NotifyOnline);
            return true;
        }
        public async Task<bool> SendMessage(ContactInfo contactInfo, string message)
        {
            await _messageProvider.SendMessage(contactInfo, message, MessageType.Message);
            return true;
        }

    }
}
