using ApplicationShare.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureShare.Services
{
    public class SocketManager : ISocketManager
    {
        private static ConcurrentDictionary<string, Socket> clients = new ConcurrentDictionary<string, Socket>();
        public SocketManager()
        {
            CloseCorruptedSocket();
        }

        public Socket FindSocket(string socketId)
        {
            if (clients.ContainsKey(socketId))
                return clients[socketId];
            return null;
        }
        public bool AddSocket(Socket socket, string socketId)
        {
            return clients.TryAdd(socketId, socket);
        }

        public bool TrySocket(string socketId)
        {
            if (clients.ContainsKey(socketId))
                return clients[socketId].Connected;
            return false;
        }

        public bool RemoveSocket(string socketId)
        {
            var result = clients.TryRemove(socketId, out var socket);
            if (socket != null)
                socket.Close();
            return result;
        }

        public bool IsExist(string socketId)
        {
            return clients.ContainsKey(socketId);
        }

        private void CloseCorruptedSocket()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {

                    Parallel.ForEach(clients, client =>
                    {
                        var socket = client.Value;

                        if (socket == null || !socket.Connected)
                        {
                            if (clients.TryRemove(client.Key, out var disconnectedSocket))
                            {
                                try
                                {
                                    disconnectedSocket.Shutdown(SocketShutdown.Both);
                                    disconnectedSocket.Close();
                                }
                                catch (SocketException)
                                {

                                }
                                finally
                                {

                                }
                            }
                        }
                    });
                    Task.Delay(1000).Wait();
                }
            });
        }
    }
}
