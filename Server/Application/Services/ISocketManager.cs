using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Services
{
    public interface ISocketManager
    {
        Socket FindSocket(string socketId);
        bool AddSocket(Socket socket,string socketId);
        bool RemoveSocket(string socketId);
        bool TrySocket(string socketId);
        bool IsExist(string socketId);
    }
}
