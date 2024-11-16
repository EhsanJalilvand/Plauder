using DomainShare.Settings;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Moq;

namespace Share.Tests.Builder
{
    public static class ServerSettingBuilder
    {
        public static IOptions<ServerSetting> Build()
        {
            var randomPort= new Random().Next(9000,14000);
            var serverSetting = new Mock<IOptions<ServerSetting>>();
            serverSetting.Setup(s=>s.Value).Returns(new ServerSetting() { Ip="127.0.0.1", Port= randomPort, ChunkSize=1024});
            return serverSetting.Object;
        }

    }
}
