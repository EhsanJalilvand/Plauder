using DomainShare.Enums;
using DomainShare.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Application.Tests.Integration
{
    public class ClientToClientMessageTransfer_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        public ClientToClientMessageTransfer_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _dataFixture.StartNewServerAndClient();
        }

        [Fact]
        public async void ClientSendMessageToAnotherClient_Test()
        {
            //Arrange
            var sender = new ContactInfo() { Id = "Id1", UserName = "User1" };
            var reciever = new ContactInfo() { Id = "Id2", UserName = "User2" };
            string message = "TestMessage";
            var registered = new TaskCompletionSource<bool>();
            var connected = new TaskCompletionSource<bool>();
            var connected2 = new TaskCompletionSource<bool>();
            var resolved = new TaskCompletionSource<bool>();
            var clientResolved = new TaskCompletionSource<bool>();
            bool isMessageCorrectlyRecieved = false;


            var newClient = new ContactInfo();
            _dataFixture.ClientMessageResolver.ResolveMessages(async (message) =>
            {
                if(message.MessageType==MessageType.NotifyOnline)
                {
                    newClient.Id =message.Sender.Id;
                    newClient.UserName = message.Sender.UserName;
                    registered.SetResult(true);
                }
                return true;
            });
            _dataFixture.ClientSrvice.Start(() =>
            {
                connected.SetResult(true);
            }, (message) =>
            {
                if (message.Reciever.Id == newClient.Id && string.Equals(message.Message, message.Message))
                {
                    isMessageCorrectlyRecieved = true;
                    clientResolved.SetResult(true);
                }
            });
            _dataFixture.ClientSrvice2.Start(() =>
            {
                connected2.SetResult(true);
            }, (message) =>
            {

            });
            await connected.Task;
            await connected2.Task;

            await _dataFixture.ClientSrvice.RegisterClient(reciever);
            await Task.Delay(3000);
            await _dataFixture.ClientSrvice2.RegisterClient(sender);
            await Task.Delay(3000);

            await registered.Task;
            await _dataFixture.ClientSrvice2.SendMessage(newClient, message);
            //Act
            await clientResolved.Task;

            //Assert
            isMessageCorrectlyRecieved.Should().BeTrue();
        }

    }
}
