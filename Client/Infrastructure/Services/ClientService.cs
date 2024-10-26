using ApplicationShare.Dtos;
using ApplicationShare.Helper;
using ApplicationShare.Settings;
using Client.Application.Services;
using Microsoft.Extensions.Options;
using Share.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureClient.Services
{
    public class ClientService : IClientService
    {
        private readonly IMessageProvider _messageProvider;
        private readonly ServerSetting _serverSetting;
        private string clientIp = "127.0.0.1";
        private int clientPort = 0;
        private ContactInfo contactInfo=null;
        public Action<MessageContract> MessageCallBack { get; set; }

        public ClientService(IMessageProvider messageProvider, IOptions<ServerSetting> options)
        {
            _messageProvider = messageProvider;
            _serverSetting = options.Value;
            clientPort = PortHelper.FreeTcpPort();
            var name = System.Environment.MachineName + ":" + clientPort;
            contactInfo=new ContactInfo() { Ip = clientIp,Port=clientPort, Name = name };
            WaitForMessage();
        }
        public void WaitForMessage()
        {
            Task.Factory.StartNew(async () => {
               await _messageProvider.RecieveMessageAsync(clientIp, clientPort, (a) =>
                {
                    MessageCallBack(a);
                });
            });

        }
        public async Task<bool> CloseSession()
        {
            await _messageProvider.SendMessageAsync(_serverSetting.Ip, _serverSetting.Port, String.Empty, ApplicationShare.Enums.MessageType.Close, contactInfo.Name,"");
            return true;
        }

        public async Task<bool> RegisterClient()
        {
            await _messageProvider.SendMessageAsync(_serverSetting.Ip, _serverSetting.Port, String.Empty, ApplicationShare.Enums.MessageType.Register, contactInfo.Name,"");
            return true;
        }

        public async Task<bool> SendMessage(string name, string message)
        {
            await _messageProvider.SendMessageAsync(_serverSetting.Ip, _serverSetting.Port, message, ApplicationShare.Enums.MessageType.Message,contactInfo.Name,name);
            return true;
        }
    }
}
