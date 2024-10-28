using ApplicationShare.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Application.Services
{
    public interface IClientService
    {
        void Start(Action connected,Action<MessageContract> MessageCallBack);
        Task<bool> RegisterClient(ContactInfo contactInfo);
        Task<bool> CloseSession();
        Task<bool> SendMessage(ContactInfo contactInfo, string message);
        //Action<MessageContract> MessageCallBack { get; set; }
        //Action Connected { get; set; }
    }
}
