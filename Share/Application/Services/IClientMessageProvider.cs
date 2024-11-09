using DomainShare.Enums;
using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface IClientMessageProvider
    {
        void Initialize(Action connected);
        Task ReceiveMessageAsync();
        Task<bool> SendMessage(ContactInfo contact, string message, MessageType messageType);


    }
}
