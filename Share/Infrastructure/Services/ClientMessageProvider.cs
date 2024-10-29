using Microsoft.Extensions.Options;
using Share.Application.Services;
using System.Net.Sockets;
using System.Net;
using System.Text;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using System;

public class ClientMessageProvider : IClientMessageProvider
{
    private static Socket _socket;
    private readonly ServerSetting _serverSetting;
    private static readonly object _lock = new object();
    private const int ChunkSize = 4;
    public ClientMessageProvider(IOptions<ServerSetting> option)
    {
        _serverSetting = option.Value;
    }
    Action ConnectedAction { get; set; }
    public void Initialize(Action connected)
    {
        ConnectedAction = connected;
        if (_socket == null || !_socket.Connected)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TryConnect();
        }
    }

    private async void TryConnect()
    {
        try
        {
            _socket.Connect(new IPEndPoint(IPAddress.Parse(_serverSetting.Ip), _serverSetting.Port));
            ConnectedAction();
        }
        catch (SocketException)
        {
            await Task.Delay(5000);
            TryConnect();
        }
    }

    private async Task ReconnectSocketAsync()
    {
        lock (_lock)
        {
            if (_socket != null && _socket.Connected)
                return;

            _socket?.Dispose();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            TryConnect();
        }
    }

    //public async Task ReceiveMessageAsync(Action<MessageContract> callback)
    //{

    //    StringBuilder data = new StringBuilder();
    //    bool messageFind = false;
    //    var buffer = new byte[4];
    //    while (true)
    //    {
    //        try
    //        {
    //            int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
    //            if (bytesRead == 0) break;

    //            data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
    //            var messages = data.ToString().Split('\n');
    //            for (int i = 0; i < messages.Length - 1; i++)
    //            {
    //                var message = messages[i].ConvertToObject<MessageContract>();
    //                if (message != null)
    //                {
    //                    messageFind = true;
    //                    callback(message);
    //                }
    //            }
    //            if (messageFind)
    //            {
    //                messageFind = false;
    //                data.Clear();
    //            }

    //        }
    //        catch (SocketException)
    //        {
    //            await ReconnectSocketAsync();
    //        }
    //        catch (Exception ex)
    //        {
    //            // Handle other exceptions if necessary
    //        }
    //        finally
    //        {
    //        }
    //    }
    //}

    public async Task ReceiveMessageAsync(Action<MessageContract> callback)
    {
        var buffer = new byte[ChunkSize];
        var data = new StringBuilder();

        while (true)
        {
            try
            {
                int bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRead == 0) break;

                data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                while (data.ToString().Contains("<EOF>"))
                {
                    var fullMessage = data.ToString();
                    var endOfMessageIndex = fullMessage.IndexOf("<EOF>", StringComparison.Ordinal);

                    var messageText = fullMessage.Substring(0, endOfMessageIndex);
                    data.Remove(0, endOfMessageIndex + "<EOF>".Length);

                    var message = messageText.ConvertToObject<MessageContract>();
                    if (message != null)
                    {
                        callback(message);
                    }
                }
            }
            catch (SocketException)
            {
                await ReconnectSocketAsync();
            }

        }
    }



    public async Task<bool> SendMessageAsync(ContactInfo contact, string message, MessageType messageType)
    {

        var standardMessage = new MessageContract() { Reciever = contact, Message = message, MessageType = messageType };
        byte[] messageSent = Encoding.UTF8.GetBytes(standardMessage.ConvertToJson());

        try
        {
            if (!_socket.Connected)
                await ReconnectSocketAsync();
            
            await _socket.SendAsync(new ArraySegment<byte>(messageSent), SocketFlags.None);
        }
        catch (SocketException)
        {
            await ReconnectSocketAsync();
            return false;
        }
        catch (Exception ex)
        {
            // Handle other exceptions if needed
            return false;
        }

        return true;
    }
}
