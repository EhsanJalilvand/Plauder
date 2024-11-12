using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Application.Services
{
    public interface IServerService
    {
        void StartService(Action<bool> callBackResult);
        void KeepLive();
        void StopService();
    }
}
