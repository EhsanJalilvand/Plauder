using DomainShare.Enums;
using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Application.Services
{
    public interface IClientMessageProvider
    {
        void Initialize(Action connected);
        Task ReceiveMessageAsync(Action<MessageContract> callback);
        Task<bool> SendMessageAsync(ContactInfo contact, string message,MessageType messageType);
    }
}
