using ApplicationShare.Services;
using Client.Application.Services;
using DomainShare.Models;
using DomainShare.Settings;
using FluentAssertions;
using InfrastructureClient.Services;
using Microsoft.Extensions.Options;
using Moq;
using Tynamix.ObjectFiller;
namespace ApplicationClient.Tests.Unit
{
    public class ClientSevice_Start_Test:IClassFixture<DataFixture>
    {
        private readonly DataFixture _dataFixture;
        private Mock<IClientMessageProvider> _mockMessageProvider;
        private Mock<IMessageResolver> _mockMessageResolver;
        private readonly IClientService _clientService;
        public ClientSevice_Start_Test(DataFixture dataFixture)
        {
            _dataFixture = dataFixture;
            _mockMessageProvider = new Mock<IClientMessageProvider>();
            _mockMessageResolver = new Mock<IMessageResolver>();
            _clientService = new ClientService(_mockMessageProvider.Object, dataFixture.ServerSettingOption, _mockMessageResolver.Object);
        }
        [Fact]
        public async Task Start_InvokesConnectedCallback()
        {
            //Arange

            bool connected = false;
            bool isMessageRecieve = false;
            var onConnectCallback = new TaskCompletionSource<bool>();
            var messageCallbackInvoked = new TaskCompletionSource<bool>();
            var messageContract = new MessageContract { Message = "Test Message" };

            _mockMessageProvider.Setup(mp => mp.StartService(It.IsAny<Action>()))
                                    .Callback<Action>(onConnect =>
                                    {
                                        onConnect();
                                        onConnectCallback.SetResult(true);
                                    });



            _mockMessageResolver.Setup(mr => mr.StartRecieve(It.IsAny<Func<MessageContract, Task<bool>>>()))
                                .Callback<Func<MessageContract, Task<bool>>>(async callback =>
                                {
                                    await callback(messageContract);
                                    messageCallbackInvoked.SetResult(true);
                                });

            _mockMessageProvider.Setup(mp => mp.ReceiveMessageAsync()).Returns(Task.CompletedTask);



            //Act
            _clientService.Start(() => { connected = true; }, (message) => { isMessageRecieve = true; });

            await Task.WhenAll(onConnectCallback.Task, messageCallbackInvoked.Task);

            //Assert

            _mockMessageProvider.Verify(mp => mp.StartService(It.IsAny<Action>()), Times.Once);
            _mockMessageResolver.Verify(mr => mr.StartRecieve(It.IsAny<Func<MessageContract, Task<bool>>>()), Times.Once);
            _mockMessageProvider.Verify(mp => mp.ReceiveMessageAsync(), Times.Once);
            connected.Should().BeTrue();
            isMessageRecieve.Should().BeTrue();
        }
        [Fact]
        public async Task Start_WhenInitializeFails_DoesNotInvokeAnyCallback()
        {
            //Arrange
            bool connectedOrMessageCallback = false;
            _mockMessageProvider.Setup(a => a.StartService(It.IsAny<Action>())).Callback<Action>(onConnect => onConnect()).Throws(new Exception("Initialize Fail"));

            //Act
            _clientService.Start(() => { connectedOrMessageCallback = true; }, (messageCallback) => { connectedOrMessageCallback = true; });
            
            //Assert
            connectedOrMessageCallback.Should().BeFalse();

        }
        [Fact]
        public async Task Start_StartRecieveFail_DoesNotInvokeMessageCallback()
        {
            //Arrane
            bool messageRecieved=false;
            var onConnectCallback =new TaskCompletionSource<bool>();


            _mockMessageProvider.Setup(a => a.StartService(It.IsAny<Action>())).Callback<Action>(onConnect =>
            {
                onConnect();
                onConnectCallback.SetResult(true);
            });
            _mockMessageResolver.Setup(a => a.StartRecieve(It.IsAny<Func<MessageContract, Task<bool>>>())).Throws(new Exception("StartRecieve Failed"));
            //Act
            _clientService.Start(() => { }, (messageContract) => {
                messageRecieved = true;
            });
            await onConnectCallback.Task;

            //Assert
            messageRecieved.Should().BeFalse();
        }
    }
}