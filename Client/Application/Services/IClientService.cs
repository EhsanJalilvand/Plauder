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
        Task<bool> RegisterClient();
        Task<bool> CloseSession();
        Task<bool> SendMessage(string name,string message);
        Action<MessageContract> MessageCallBack { get; set; }
    }
}
