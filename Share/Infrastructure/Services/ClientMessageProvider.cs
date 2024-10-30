using Microsoft.Extensions.Options;
using Share.Application.Services;
using System.Net.Sockets;
using System.Net;
using System.Text;
using DomainShare.Settings;
using DomainShare.Models;
using DomainShare.Enums;
using System;
using ApplicationShare.Services;
using System.Text.Json;

public class ClientMessageProvider : IClientMessageProvider
{
    private static Socket _socket;
    private readonly ServerSetting _serverSetting;
    private static readonly object _lock = new object();
    private readonly IMessageQueueManager _messageQueueManager;
    private readonly IMessageResolver _messageResolver;
    public ClientMessageProvider(IOptions<ServerSetting> option, IMessageQueueManager messageQueueManager, IMessageResolver messageResolver)
    {
        _serverSetting = option.Value;
        _messageQueueManager = messageQueueManager;
        _messageResolver = messageResolver;
    }
    Action ConnectedAction { get; set; }
    public void Initialize(Action connected)
    {
        ConnectedAction = connected;
        if (_socket == null || !_socket.Connected)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TryConnect();

            _messageQueueManager.StartSend(async (a) =>
            {
                try
                {
                    if (!_socket.Connected)
                        await ReconnectSocketAsync();
                    var message = JsonSerializer.Serialize(a) + "<EOF>";
                    byte[] binaryChunk = Encoding.UTF8.GetBytes(message);
                    await _socket.SendAsync(new ArraySegment<byte>(binaryChunk), SocketFlags.None);
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


            });

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


    public async Task ReceiveMessageAsync()
    {

        var buffer = new byte[_serverSetting.ChunkSize];
        var data = new StringBuilder();

        while (true)
        {
            try
            {
                int bytesRead = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRead == 0) break;

                data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                var messageText=data.ToString();
                var messages = messageText.Split("<EOF>");
                foreach (var message in messages)
                {
                    if (string.IsNullOrEmpty(message)) continue;
                    var chunkMessage = message.ConvertToObject<MessageChunk>();
                    _messageResolver.ReadChunkMessage(chunkMessage);
                }
            }
            catch (SocketException)
            {
                await ReconnectSocketAsync();
            }


        }
}



    public async Task<bool> SendMessage(ContactInfo contact, string message, MessageType messageType)
    {
        var standardMessage = new MessageContract() { Reciever = contact, Message = message, MessageType = messageType };
        _messageQueueManager.PushToQueue(standardMessage);
        return true;
    }
}
