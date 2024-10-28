using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Share.Application.Services;
using ApplicationShare.Enums;
using ApplicationShare.Dtos;
using System.Collections.Concurrent;
using ApplicationShare.Settings;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Reflection;

namespace Share.Infrastructure.Services
{
    public class ServerMessageProvider : IServerMessageProvider
    {
        private static ConcurrentDictionary<string, Socket> clients = new ConcurrentDictionary<string, Socket>();
        private readonly ServerSetting _serverSetting;
        public ServerMessageProvider(IOptions<ServerSetting> option)
        {
            _serverSetting = option.Value;
        }
        public Task ListenMessageAsync(Action<MessageContract> callback)
        {
            // Establish the local endpoint 
            // for the socket. Dns.GetHostName
            // returns the name of the host 
            // running the application.
            IPHostEntry iPHost = Dns.GetHostEntry(_serverSetting.Ip);
            IPAddress iPaddress = iPHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPaddress, _serverSetting.Port);
            Socket socket = new Socket(iPaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(localEndPoint);
                // Using Listen() method we create 
                // the Client list that will want
                // to connect to Server
                socket.Listen(10);
                while (true)
                {
                    // Suspend while waiting for
                    // incoming connection Using 
                    // Accept() method the server 
                    // will accept connection of client
                    Socket clientSocket = socket.Accept();
                    IPEndPoint remoteEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;
                    string clientIP = remoteEndPoint?.Address.ToString();
                    int clientPort = remoteEndPoint?.Port ?? 0;
                    string clientId = $"{clientIP}:{clientPort}";
                    if (!clients.ContainsKey(clientId))
                    {
                        clients.TryAdd(clientId, clientSocket);
                        Task.Run(() => HandleClient(clientSocket, clientId, callback));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        private static async Task HandleClient(Socket client, string clientId, Action<MessageContract> callback)
        {
            byte[] buffer = new byte[1024];
            StringBuilder data = new StringBuilder();
            try
            {

                while (clients.ContainsKey(clientId))
                {
                    int numByte = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (numByte == 0) break; // کلاینت قطع شده است

                    data.Append(Encoding.UTF8.GetString(buffer, 0, numByte));
                    // پردازش پیام کامل
                    var message = data.ToString().ConvertToObject<MessageContract>();
                    if (message != null)
                    {
                        data.Clear();
                        message.Sender = new ContactInfo() {Id=clientId };
                        callback(message);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                client.Close();
                clients.TryRemove(clientId, out _);
            }
        }

        public async Task<bool> SendMessageAsync(ContactInfo sender, ContactInfo receiver, string message, MessageType messageType)
        {
            if (!clients.TryGetValue(receiver.Id, out var socket) || socket == null || !socket.Connected)
            {
                clients.TryRemove(receiver.Id, out _);
                return false;
            }
            socket = clients[receiver.Id];
            try
            {
                var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                var standardMessage = new MessageContract() { Sender=sender,Reciever = receiver, Message = message, MessageType = messageType };
                byte[] messageSent = Encoding.UTF8.GetBytes(standardMessage.ConvertToJson());
                socket.Send(messageSent);

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> RemoveClientAsync(ContactInfo sender)
        {
            var socket = clients[sender.Id];
            socket.Close();
            clients.TryRemove(sender.Id, out _);
            return true;
        }
    }
}
