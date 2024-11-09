using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Reflection;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using ApplicationShare.Services;
using InfrastructureShare.Services;
using System.Text.Json;

namespace InfrastructureShare.Services

{
    public class ServerMessageProvider : IServerMessageProvider
    {
        private static ConcurrentDictionary<string, Socket> clients = new ConcurrentDictionary<string, Socket>();
        private readonly ServerSetting _serverSetting;
        private readonly IMessageQueueManager _queueManager;
        private readonly IMessageResolver _messageResolver;
        public ServerMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager queueManager, IMessageResolver messageResolver)
        {
            _serverSetting = option.Value;
            Task.Factory.StartNew(() =>
            {
                CloseCorruptedSocket();
            });
            _queueManager = queueManager;
            _messageResolver = messageResolver;


            _queueManager.StartSend(async (a) =>
            {
                try
                {
                   var _socket = clients[a.RecieverId];
                    if (!_socket.Connected)
                        return false;
                    var message = JsonSerializer.Serialize(a) + "<EOF>";
                    byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
                    await _socket.SendAsync(new ArraySegment<byte>(binaryChunk), SocketFlags.None);
                }
                catch (SocketException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    // Handle other exceptions if needed
                    return false;
                }

                return true;


            });

        }
        private void CloseCorruptedSocket()
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
            _messageResolver.StartRecieve(async(a) =>
            {
                callback(a);
                return true;
            });
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
                        Task.Run(() => HandleClient(clientSocket, clientId));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        private async Task HandleClient(Socket client, string clientId)
        {
            byte[] buffer = new byte[_serverSetting.ChunkSize];
            StringBuilder data = new StringBuilder();
            try
            {
                while (clients.ContainsKey(clientId))
                {
                    try
                    {
                        int numByte = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                        if (numByte == 0) break;

                        data.Append(Encoding.UTF8.GetString(buffer, 0, numByte));
                        var messageText = data.ToString();
                        if(!messageText.EndsWith("<EOF>"))
                            continue;
                        var messages=messageText.Split("<EOF>");
                        foreach (var message in messages)
                        {
                            if(string.IsNullOrEmpty(message)) continue;
                            var chunkMessage = message.ConvertToObject<MessageChunk>();
                            chunkMessage.ClientId = clientId;
                            _messageResolver.ReadChunkMessage(chunkMessage);
                        }
                        data.Clear();
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
                client.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                clients.TryRemove(clientId, out _);
            }
        }


        public async Task<bool> RemoveClientAsync(ContactInfo sender)
        {
            var socket = clients[sender.Id];
            socket.Close();
            clients.TryRemove(sender.Id, out _);
            return true;
        }

        public async Task<bool> SendMessage(ContactInfo sender, ContactInfo receiver, string message, MessageType messageType)
        {
            if (!clients.TryGetValue(receiver.Id, out var socket))
            {
                return false;
            }
            var standardMessage = new MessageContract { Sender = sender, Reciever = receiver, Message = message, MessageType = messageType };
            _queueManager.PushToQueue(standardMessage);
            return true;
        }
    }
}
