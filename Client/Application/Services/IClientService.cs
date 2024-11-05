using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Application.Services
{
    public interface IClientService
    {
        void Start(Action connected,Action<MessageContract> messageCallback);
        Task<bool> RegisterClient(ContactInfo contactInfo);
        Task<bool> UnRegisterClient();
        Task<bool> SendMessage(ContactInfo contactInfo, string message);
    }
}
