using ApplicationShare.Dtos;
using ChatClient.ViewModels;
using Client.Application.Services;
using DomainShare.Enums;
using DomainShare.Models;

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
            this.BindingContext = _mainViewModel;
        }
        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            _chatClient.Start(GetName,RecieveNewMessage);
        }
        private void GetName()
        {
            this.Dispatcher.Dispatch(async () =>
            {
                string result = await DisplayPromptAsync("Owner", "What's your name?");
                this.Title = result;
                await _chatClient.RegisterClient(new ContactInfo() { UserName=result});
            });
        }
        public async void RecieveNewMessage(MessageContract messageContract)
        {
            //this.Dispatcher.Dispatch(() =>
            //{
                if (messageContract.MessageType == MessageType.NotifyOnline && !_mainViewModel.Data.ContactInfos.Any(a => a.ID == messageContract.Sender.Id))
                _mainViewModel.Data.ContactInfos.Add(new ApplicationShare.Dtos.ContactModel() { ID = messageContract.Sender.Id, Name = messageContract.Sender.UserName });
            if (messageContract.MessageType == MessageType.NotifyOffline && _mainViewModel.Data.ContactInfos.Any(a => a.ID == messageContract.Sender.Id))
                _mainViewModel.Data.ContactInfos.Remove(_mainViewModel.Data.ContactInfos.First(a => a.ID == messageContract.Sender.Id));
            if (messageContract.MessageType == MessageType.Message)
            {
                var contact = _mainViewModel.Data.ContactInfos.FirstOrDefault(p => p.ID == messageContract.Sender.Id);
                contact.Messages.Add(new Models.MessageModel() { Text = messageContract.Message });
            }
            //});
        }
        private async void ContentPage_Unloaded(object sender, EventArgs e)
        {
            await _chatClient.CloseSession();
        }
    }

}
