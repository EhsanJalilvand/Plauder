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
    public class ServerToClientMessageTransfer_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        public ServerToClientMessageTransfer_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _dataFixture.StartNewServerAndClient();
        }


        [Fact]
        public async Task Start_RegisterClient_ShouldBeEstablisched()
        {
            //Arrange
            bool isNotifyMessageRecieved = false;
            var contactInfo = new ContactInfo() { Id = "Id01", UserName = "User1" };
            var connected = new TaskCompletionSource<bool>();
            var resolved = new TaskCompletionSource<bool>();
            _dataFixture.ServerMessageResolver.ResolveMessages(async (a) =>
            {
                if (a.MessageType == DomainShare.Enums.MessageType.NotifyOnline)
                {
                    isNotifyMessageRecieved = true;
                    resolved.SetResult(true);
                }
                return true;
            });
            _dataFixture.ClientSrvice.Start(() =>
            {
                connected.SetResult(true);
            }, (message) =>
            {

            });
            await connected.Task;
            //Act
            await _dataFixture.ClientSrvice.RegisterClient(contactInfo);
            //Assert
            await resolved.Task;

            isNotifyMessageRecieved.Should().BeTrue();

        }


    }
}
