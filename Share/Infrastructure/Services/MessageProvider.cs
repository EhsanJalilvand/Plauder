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

namespace Share.Infrastructure.Services
{
    public class MessageProvider : IMessageProvider
    {
        public Task RecieveMessageAsync(string ip, int port, Action<MessageContract> callback)
        {
            // Establish the local endpoint 
            // for the socket. Dns.GetHostName
            // returns the name of the host 
            // running the application.
            IPHostEntry iPHost = Dns.GetHostEntry(ip);
            IPAddress iPaddress = iPHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(iPaddress, port);
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

                    //IPEndPoint remoteEndPoint = clientSocket.LocalEndPoint as IPEndPoint;
                    //string clientIP = remoteEndPoint?.Address.ToString();
                    //int clientPort = remoteEndPoint?.Port ?? 0;


                    byte[] buffer = new byte[1024];
                    string data = null;
                    while (true)
                    {
                        int numByte = clientSocket.Receive(buffer);

                        data += Encoding.UTF8.GetString(buffer, 0, numByte);
                        var message = data.ConvertToObject<MessageContract>();
                        if (message != null)
                        {

                            callback(message);
                            break;
                        }
                    }
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();


                }
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }
        public async Task<bool> SendMessageAsync(string ip, int port, string message, MessageType messageType,string sender,string reciever)
        {
            // Establish the remote endpoint 
            // for the socket. This example 
            IPHostEntry iPHost = Dns.GetHostEntry(ip);
            IPAddress iPaddress = iPHost.AddressList[0];
            IPEndPoint remoteEndPoint = new IPEndPoint(iPaddress, port);

            Socket socket = new Socket(iPaddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                socket.Connect(remoteEndPoint);
                var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                //var standardMessage = new MessageContract() { Message = message, IPAddress = localEndPoint?.AddressFamily.ToString(), Port = localEndPoint.Port,MessageType= messageType };
                var standardMessage = new MessageContract() { Sender= sender,Reciever=reciever, Message = message, MessageType = messageType };
                byte[] messageSent = Encoding.UTF8.GetBytes(standardMessage.ConvertToJson());
                socket.Send(messageSent);


                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
