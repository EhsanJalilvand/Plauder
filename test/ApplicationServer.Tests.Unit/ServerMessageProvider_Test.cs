using ApplicationShare.Services;
using DomainShare.Models;
using InfrastructureShare.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationServer.Tests.Unit
{
    public class ServerMessageProvider_Test : IClassFixture<DataFixture>
    {
        private readonly DataFixture dataFixture;
        private readonly IServerMessageProvider _serverMessageProvider;
        private readonly Mock<IMessageQueueManager> _moqQueueManager;
        private readonly Mock<IMessageResolver> _moqMessageResolver;
        private readonly Mock<ISocketServerProvider> _moqSoketProvider;
        private readonly Mock<ISocketManager> _moqSoketManager;
        public ServerMessageProvider_Test(DataFixture dataFixture)
        {
            this.dataFixture = dataFixture;
            _moqQueueManager = new Mock<IMessageQueueManager>();
            _moqMessageResolver = new Mock<IMessageResolver>();
            _moqSoketProvider = new Mock<ISocketServerProvider>();
            _moqSoketManager=new Mock<ISocketManager>();
            _serverMessageProvider = new ServerMessageProvider(dataFixture.ServerSettingOption, _moqQueueManager.Object, _moqMessageResolver.Object, _moqSoketProvider.Object, _moqSoketManager.Object);
        }
        [Fact]
        public void SendQueueMessagesToClients_ShouldSendAllMessageInQueue()
        {
            //Arrange
            MessageChunk mc1 = new MessageChunk() { ChunkNumber = 0, RecieverId = "Id0", Message = Encoding.UTF8.GetBytes("Test1"), MessageId = Guid.NewGuid() };
            MessageChunk mc2 = new MessageChunk() { ChunkNumber = 4, RecieverId = "Id1", Message = Encoding.UTF8.GetBytes("Test2"), MessageId = Guid.NewGuid() };
            MessageChunk mc3 = new MessageChunk() { ChunkNumber = 1, RecieverId = "Id2", Message = Encoding.UTF8.GetBytes("Test3"), MessageId = Guid.NewGuid() };

            _moqSoketManager.Setup(a => a.TrySocket(It.IsAny<string>())).Returns(true);
            _moqQueueManager.Setup(a => a.StartSend(It.IsAny<Func<MessageChunk, Task<bool>>>()))
                .Callback<Func<MessageChunk, Task<bool>>>(async (f) =>
                {
                    await f(mc1);
                    await f(mc2);
                    await f(mc3);
                });



            //Act
            _serverMessageProvider.SendQueueMessagesToClients();
            //Assert
            _moqSoketProvider.Verify(a => a.SendAsync(It.IsAny<string>(),It.IsAny<MessageChunk>()), Times.Exactly(3));
        }

        //[Fact]
        //public void ListenMessageAsync_ShouldReadChunkMessage()
        //{
        //    //Arrang
        //    var s1 = _moqSoketManager;
        //    _moqSoketProvider.Setup(a => a.ListenAsync(It.IsAny<Action<Socket, string>>())).Callback<Action<Socket, string>>((action) =>
        //    {
        //        action()
        //    });

        //    //Act
        //    _serverMessageProvider.ListenMessageAsync();


        //    //Assert
        //}
    }
}
