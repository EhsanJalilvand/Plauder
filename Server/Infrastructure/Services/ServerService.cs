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

namespace Server.Infrastructure.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerMessageProvider _messageProvider;
        private readonly ICollection<ContactInfo> _contactInfos = new Collection<ContactInfo>();
        public ServerService(IServerMessageProvider messageProvider, IOptions<ServerSetting> options)
        {
            _messageProvider = messageProvider;
        }
        public async Task<bool> RegisterClient(MessageContract messageContract)
        {

            _contactInfos.Add(new ContactInfo() { Id = messageContract.Sender.Id, UserName = messageContract.Message});


            foreach (var contactInfo in _contactInfos)
            {
                foreach (var item in _contactInfos)
                {
                    if (item.Id != contactInfo.Id)
                    {
                        await _messageProvider.SendMessage(contactInfo, item, "", MessageType.NotifyOnline);
                    }
                }
            }

            return true;
        }
        public async Task<bool> RemoveClient(MessageContract messageContract)
        {

            await _messageProvider.RemoveClientAsync(messageContract.Sender);
            var contract = _contactInfos.FirstOrDefault(p => p.Id == messageContract.Sender.Id);
            if (contract != null)
            {
                _contactInfos.Remove(contract);
                foreach (var item in _contactInfos)
                {
                    await _messageProvider.SendMessage(messageContract.Sender, item, messageContract.Message, MessageType.NotifyOffline);
                }
            }
            return true;
        }
        public async Task<bool> SendMessage(MessageContract messageContract)
        {
            var contact = _contactInfos.FirstOrDefault(s => s.Id == messageContract.Reciever.Id);
            if (contact == null)
                return false;

            await _messageProvider.SendMessage(messageContract.Sender,contact, messageContract.Message, MessageType.Message);

            return true;
        }
        public void StartService(Action<bool> callBackResult)
        {
            bool result=false;
            _messageProvider.ListenMessageAsync(async (a) =>
            {
                 if (a.MessageType == MessageType.NotifyOnline)
                    result= await RegisterClient(a);
                else if (a.MessageType == MessageType.NotifyOffline)
                    result = await RemoveClient(a);
                else if (a.MessageType == MessageType.Message)
                    result = await SendMessage(a);
                callBackResult(result);  
            });
           
        }
    }
}
