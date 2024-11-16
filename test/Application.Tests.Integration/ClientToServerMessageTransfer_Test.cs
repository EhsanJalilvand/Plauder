using DomainShare.Models;
using FluentAssertions;
using InfrastructureShare.Services;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Server.Application.Services;
using System;
using System.Net.Http.Headers;

namespace Application.Tests.Integration
{
    public class ClientToServerMessageTransfer_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        public ClientToServerMessageTransfer_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _dataFixture.StartNewServerAndClient();
        }
        [Fact]
        public async void Start_CommunicationBetweenServerAndClient_ShouldBeEstablisched()
        {

            //Arrange
            bool isClientToServerConnected = false;
            var tcs = new TaskCompletionSource<bool>();
            //Act
            _dataFixture.ClientSrvice.Start(() =>
            {
                isClientToServerConnected = true;
                tcs.SetResult(true);
            }, (message) =>
            {

            });
            await tcs.Task;
            //Assert
            isClientToServerConnected.Should().BeTrue();
        }
        [Fact]
        public async void Start_RegisterClient_ShouldBeEstablisched()
        {
            //Arrange
            bool isNotifyMessageRecieved = false;
            var connected = new TaskCompletionSource<bool>();
            var tcs = new TaskCompletionSource<bool>();
            var contactInfo = new ContactInfo() { Id = "Id01", UserName = "User1" };


            //Act & Assert
            _dataFixture.ServerMessageResolver.ResolveMessages(async (a) =>
            {
                if (a.MessageType == DomainShare.Enums.MessageType.NotifyOnline)
                {
                    isNotifyMessageRecieved = true;
                    tcs.SetResult(true);
                }
                return true;
            });
            _dataFixture.ClientSrvice.Start(() =>
            {
                connected.SetResult(true);
            }, (message) =>
            {

            });

            await _dataFixture.ClientSrvice.RegisterClient(contactInfo);
            await connected.Task;
            await tcs.Task;
            isNotifyMessageRecieved.Should().BeTrue();

        }
        [Fact]
        public async void Start_UnRegisterClient_ShouldBeEstablisched()
        {
            //Arrange
            bool isNotifyMessageRecieved = false;
            var contactInfo = new ContactInfo() { Id = "Id02", UserName = "User2" };
            var tcs = new TaskCompletionSource<bool>();
            var connected = new TaskCompletionSource<bool>();
            _dataFixture.ServerMessageResolver.ResolveMessages(async (a) =>
            {
                if (a.MessageType == DomainShare.Enums.MessageType.NotifyOffline)
                {
                    isNotifyMessageRecieved = true;
                    tcs.SetResult(true);
                }
                return true;
            });
            _dataFixture.ClientSrvice.Start(() =>
            {
                connected.SetResult(true);
            }, (message) =>
            {

            });
            //Act
            await connected.Task;
            await _dataFixture.ClientSrvice.RegisterClient(contactInfo);
            await Task.Delay(3000);
            await _dataFixture.ClientSrvice.UnRegisterClient();
            await tcs.Task;
            //Assert
            isNotifyMessageRecieved.Should().BeTrue();


        }
        [Fact]
        public async void Start_SendMessage_ShouldBeEstablisched()
        {
            //Arrange
            bool isNotifyMessageRecieved = false;
            var contactInfo = new ContactInfo() { Id = "Id03", UserName = "User1" };
            string message = "TestMessage";
            var connected = new TaskCompletionSource<bool>();
            var tcs = new TaskCompletionSource<bool>();
            _dataFixture.ServerMessageResolver.ResolveMessages(async (a) =>
            {
                if (a.MessageType == DomainShare.Enums.MessageType.Message)
                {
                    isNotifyMessageRecieved = a.Message == message;
                    tcs.SetResult(true);
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
            await _dataFixture.ClientSrvice.SendMessage(contactInfo, message);
            //Assert
            await tcs.Task;
            isNotifyMessageRecieved.Should().BeTrue();

        }

        public async void Dispose()
        {
            //await Task.Delay(1000);
            _dataFixture.ServerSrvice.StopService();
        }
    }
}