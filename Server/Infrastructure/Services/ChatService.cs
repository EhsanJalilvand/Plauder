using ApplicationShare.Dtos;
using ApplicationShare.Settings;
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
    public class ChatService : IChatService
    {
        private readonly IMessageProvider _messageProvider;
        private readonly ServerSetting _serverSetting;
        private readonly IMemoryCache _memoryCache;
        private readonly ICollection<ContactInfo> _contactInfos = new Collection<ContactInfo>();
        public ChatService(IMessageProvider messageProvider, IOptions<ServerSetting> options, IMemoryCache memoryCache)
        {
            _messageProvider = messageProvider;
            _serverSetting = options.Value;
            _memoryCache = memoryCache;
        }
        public async Task<bool> RegisterClient(MessageContract messageContract)
        {
            var key = messageContract.Sender;
            MessageContract mc = null;
            if (!_memoryCache.TryGetValue(key, out mc))
                _memoryCache.Set(key, messageContract);

            _contactInfos.Add(new ContactInfo() { Name=messageContract.Sender,Ip="127.0.0.1",Port=int.Parse(messageContract.Sender.Substring(messageContract.Sender.IndexOf(":")+1)) });


            foreach (var contactInfo in _contactInfos)
            {
                foreach (var item in _contactInfos)
                {
                    if(item.Name !=contactInfo.Name)
                    await _messageProvider.SendMessageAsync(item.Ip, item.Port, item.Name, ApplicationShare.Enums.MessageType.NotifyOnline, contactInfo.Name,item.Name);
                }
            }

            return true;
        }
        public async Task<bool> RemoveClient(MessageContract messageContract)
        {
            var key = messageContract.Sender;
            MessageContract mc = null;
            if (_memoryCache.TryGetValue(key, out mc))
                _memoryCache.Remove(key);

            var contract = _contactInfos.FirstOrDefault(p => p.Name == messageContract.Sender);
            if (contract != null)
            {
                _contactInfos.Remove(contract);
                foreach (var item in _contactInfos)
                {
                    await _messageProvider.SendMessageAsync(item.Ip, item.Port, messageContract.Sender, ApplicationShare.Enums.MessageType.NotifyOffline, messageContract.Sender,messageContract.Reciever);
                }
            }
            return true;
        }
        public async Task<bool> SendMessage(MessageContract messageContract)
        {
            var contatct= _contactInfos.FirstOrDefault(s => s.Name == messageContract.Reciever);
            if(contatct==null)
                return false;

            await _messageProvider.SendMessageAsync(contatct.Ip, contatct.Port, messageContract.Message, ApplicationShare.Enums.MessageType.Message, messageContract.Reciever,messageContract.Sender);

            return true;
        }
        public Task StartService()
        {
            _messageProvider.RecieveMessageAsync(_serverSetting.Ip, _serverSetting.Port, async (a) =>
            {
                if (a.MessageType == ApplicationShare.Enums.MessageType.Register)
                    await RegisterClient(a);
                else if (a.MessageType == ApplicationShare.Enums.MessageType.Close)
                    await RemoveClient(a);
                else if (a.MessageType == ApplicationShare.Enums.MessageType.Message)
                    await SendMessage(a);
            });
            return Task.CompletedTask;
        }
    }
}
