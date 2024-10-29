using ApplicationShare.Dtos;
using ApplicationShare.Enums;
using ApplicationShare.Settings;
using Microsoft.Extensions.Options;
using Share.Application.Services;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class ClientMessageProvider : IClientMessageProvider
{
    private static Socket _socket;
    private readonly ServerSetting _serverSetting;
    private static readonly object _lock = new object();

    public ClientMessageProvider(IOptions<ServerSetting> option)
    {
        _serverSetting = option.Value;
    }
    Action ConnectedAction { get; set; }
    public void Initialize(Action connected)
    {
        ConnectedAction= connected;
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

    public async Task ReceiveMessageAsync(Action<MessageContract> callback)
    {
        byte[] buffer = new byte[1024];
        var data = new StringBuilder();

        while (true)
        {
            try
            {
                int bytesRead = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0) break;

                data.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                var message = data.ToString().ConvertToObject<MessageContract>();
                if (message != null)
                {
                    data.Clear();
                    callback(message);
                }
            }
            catch (SocketException)
            {
                await ReconnectSocketAsync();
            }
            catch (Exception ex)
            {
                // Handle other exceptions if necessary
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
