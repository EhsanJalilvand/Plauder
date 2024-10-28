using ApplicationShare.Dtos;
using ApplicationShare.Helper;
using ApplicationShare.Settings;
using Client.Application.Services;
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
        public ClientService(IClientMessageProvider messageProvider, IOptions<ServerSetting> options)
        {
            _messageProvider = messageProvider;
        }

        public void Start(Action connected, Action<MessageContract> MessageCallBack)
        {
            Task.Factory.StartNew(() =>
            {
                _messageProvider.Initialize(async () =>
                {
                    connected();
                    await StartRecieveMessage(MessageCallBack);
                });
            });
        }
        private async Task StartRecieveMessage(Action<MessageContract> MessageCallBack)
        {
            await _messageProvider.ReceiveMessageAsync((a) =>
            {
                MessageCallBack(a);
            });
        }
        public async Task<bool> CloseSession()
        {
            await _messageProvider.SendMessageAsync(null, String.Empty, ApplicationShare.Enums.MessageType.NotifyOffline);
            return true;
        }
        public async Task<bool> RegisterClient(ContactInfo contactInfo)
        {
            await _messageProvider.SendMessageAsync(contactInfo, contactInfo.UserName, ApplicationShare.Enums.MessageType.NotifyOnline);
            return true;
        }
        public async Task<bool> SendMessage(ContactInfo contactInfo, string message)
        {
            await _messageProvider.SendMessageAsync(contactInfo, message, ApplicationShare.Enums.MessageType.Message);
            return true;
        }

    }
}
