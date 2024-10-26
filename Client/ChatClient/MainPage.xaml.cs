using ApplicationShare.Dtos;
using ChatClient.ViewModels;
using Client.Application.Services;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {

        private readonly IClientService _chatClient;
        private readonly MainViewModel _mainViewModel;
        public MainPage(IClientService chatClient, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _chatClient = chatClient;
            _mainViewModel = mainViewModel;
            _chatClient.MessageCallBack = RecieveNewMessage;
            this.BindingContext = _mainViewModel;
        }
        public async void RecieveNewMessage(MessageContract messageContract)
        {
            //this.Dispatcher.Dispatch(() =>
            //{
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.NotifyOnline && !_mainViewModel.Data.ContactInfos.Any(a => a.Name == messageContract.Sender))
                    _mainViewModel.Data.ContactInfos.Add(new ApplicationShare.Dtos.ContactModel() { Ip = "127.0.0.1", Name = messageContract.Sender });
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.NotifyOffline && _mainViewModel.Data.ContactInfos.Any(a => a.Name == messageContract.Sender))
                    _mainViewModel.Data.ContactInfos.Remove(_mainViewModel.Data.ContactInfos.First(a => a.Name == messageContract.Sender));
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.Message)
                {
                    var contact = _mainViewModel.Data.ContactInfos.FirstOrDefault(p => p.Name == messageContract.Reciever);
                    contact.Messages.Add(new Models.MessageModel() { Text = messageContract.Message });
                }
            //});
        }
        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            await _chatClient.RegisterClient();
        }

        private async void ContentPage_Unloaded(object sender, EventArgs e)
        {
            await _chatClient.CloseSession();
        }
    }

}
