using ApplicationShare.Dtos;
using ApplicationShare.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.Application.Services
{
    public interface IMessageProvider
    {
        Task<bool> SendMessageAsync(string ip, int port, string message,MessageType messageType, string sender, string reciever);
        Task RecieveMessageAsync(string ip, int port,Action<MessageContract> callback);
    }
}
