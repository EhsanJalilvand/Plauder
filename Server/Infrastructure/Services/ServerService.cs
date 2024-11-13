using ApplicationShare.Services;
using DomainShare.Enums;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Server.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerMessageProvider _messageProvider;
        private readonly IMessageResolver _messageResolver;
        private readonly ICollection<ContactInfo> _contactInfos = new Collection<ContactInfo>();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _token;
        public ServerService(IServerMessageProvider messageProvider, IMessageResolver messageResolver)
        {
            _messageProvider = messageProvider;
            _messageResolver = messageResolver;
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
        }
        public async Task<bool> RegisterClient(MessageContract messageContract)
        {

            _contactInfos.Add(new ContactInfo() { Id = messageContract.Sender.Id, UserName = messageContract.Message });
            List<bool> lists = new List<bool>();
            foreach (var contactInfo in _contactInfos)
            {
                foreach (var item in _contactInfos)
                {
                    if (item.Id != contactInfo.Id)
                    {
                        lists.Add(await _messageProvider.SendMessageAsync(contactInfo, item, "", MessageType.NotifyOnline));
                    }
                }
            }

            return  lists.All(a => a == true);
        }
        public async Task<bool> RemoveClient(MessageContract messageContract)
        {
            await _messageProvider.RemoveClientSession(messageContract.Sender);
            var contract = _contactInfos.FirstOrDefault(p => p.Id == messageContract.Sender.Id);
            List<bool> lists = new List<bool>();
            if (contract != null)
            {
                _contactInfos.Remove(contract);
                foreach (var item in _contactInfos)
                {
                    lists.Add(await _messageProvider.SendMessageAsync(messageContract.Sender, item, messageContract.Message, MessageType.NotifyOffline));
                }
            }
            return  lists.All(a => a == true);
        }
        public async Task<bool> SendMessage(MessageContract messageContract)
        {
            var contact = _contactInfos.FirstOrDefault(s => s.Id == messageContract.Reciever.Id);
            if (contact == null)
                return false;
           return await _messageProvider.SendMessageAsync(messageContract.Sender, contact, messageContract.Message, MessageType.Message);
        }
        public async void StartService(Action<bool> callBackResult)
        {
            _messageProvider.SendQueueMessagesToClients();
            await _messageProvider.ListenMessageAsync();


            bool result = false;
            _messageResolver.ResolveMessages(async (a) =>
            {
                try
                {
                    result = false;
                    if (a.MessageType == MessageType.NotifyOnline)
                        result = await RegisterClient(a);
                    else if (a.MessageType == MessageType.NotifyOffline)
                        result = await RemoveClient(a);
                    else if (a.MessageType == MessageType.Message)
                        result = await SendMessage(a);
                    callBackResult(result);
                    return result;
                }
                catch (Exception e)
                {
                    return false;
                }
            });

        }
        public void KeepLive()
        {
            while (!_token.IsCancellationRequested)
            {
                Task.Delay(1000, _token).GetAwaiter().GetResult();
            }
        }
        public void StopService()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
