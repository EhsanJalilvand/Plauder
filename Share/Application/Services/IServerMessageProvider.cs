
using DomainShare.Enums;
using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface IServerMessageProvider
    {
        void SendQueueMessagesToClients();
        Task ListenMessageAsync();
        Task<bool> RemoveClientSession(ContactInfo sender);
        Task<bool> SendMessageAsync(ContactInfo sender, ContactInfo receiver, string message, MessageType messageType);
    }
}
