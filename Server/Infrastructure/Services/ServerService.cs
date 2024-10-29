using DomainShare.Enums;
using DomainShare.Models;
using DomainShare.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Server.Application.Services;
using Share.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Infrastructure.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerMessageProvider _messageProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly ICollection<ContactInfo> _contactInfos = new Collection<ContactInfo>();
        public ServerService(IServerMessageProvider messageProvider, IOptions<ServerSetting> options, IMemoryCache memoryCache)
        {
            _messageProvider = messageProvider;
            _memoryCache = memoryCache;
        }
        public async Task<bool> RegisterClient(MessageContract messageContract)
        {
            MessageContract mc = null;
            if (!_memoryCache.TryGetValue(messageContract.Sender, out mc))
                _memoryCache.Set(messageContract.Sender, messageContract);

            _contactInfos.Add(new ContactInfo() { Id = messageContract.Sender.Id, UserName = messageContract.Message});


            foreach (var contactInfo in _contactInfos)
            {
                foreach (var item in _contactInfos)
                {
                    if (item.Id != contactInfo.Id)
                    {
                        await _messageProvider.SendMessageAsync(contactInfo, item, "", MessageType.NotifyOnline);
                    }
                }
            }

            return true;
        }
        public async Task<bool> RemoveClient(MessageContract messageContract)
        {
            MessageContract mc = null;
            if (_memoryCache.TryGetValue(messageContract.Sender, out mc))
                _memoryCache.Remove(messageContract.Sender);

            await _messageProvider.RemoveClientAsync(messageContract.Sender);
            var contract = _contactInfos.FirstOrDefault(p => p.Id == messageContract.Sender.Id);
            if (contract != null)
            {
                _contactInfos.Remove(contract);
                foreach (var item in _contactInfos)
                {
                    await _messageProvider.SendMessageAsync(messageContract.Sender, item, messageContract.Message, MessageType.NotifyOffline);
                }
            }
            return true;
        }
        public async Task<bool> SendMessage(MessageContract messageContract)
        {
            var contact = _contactInfos.FirstOrDefault(s => s.Id == messageContract.Reciever.Id);
            if (contact == null)
                return false;

            await _messageProvider.SendMessageAsync(messageContract.Sender,contact, messageContract.Message, MessageType.Message);

            return true;
        }
        public Task StartService()
        {
            _messageProvider.ListenMessageAsync(async (a) =>
            {
                 if (a.MessageType == MessageType.NotifyOnline)
                    await RegisterClient(a);
                else if (a.MessageType == MessageType.NotifyOffline)
                    await RemoveClient(a);
                else if (a.MessageType == MessageType.Message)
                    await SendMessage(a);
            });
            return Task.CompletedTask;
        }
    }
}
