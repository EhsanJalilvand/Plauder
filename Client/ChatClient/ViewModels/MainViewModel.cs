using ApplicationShare.Dtos;
using ChatClient.Models;
using Client;
using Client.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DomainShare.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private MainScreen _mainScreen;
        private readonly IClientService _chatClient;
        private ContactModel _currentContact;
        private string _message;
        public MainViewModel(IClientService chatClient)
        {
            _chatClient = chatClient;
            _mainScreen = new MainScreen();
            SendCommand = new RelayCommand(ExecuteSendCommand, CanExecuteSendCommand);
        }
        public ICommand SendCommand { get; set; }
        public MainScreen Data
        {
            get => _mainScreen;
            set
            {
                _mainScreen = value;
            }
        }
        public ContactModel CurrentContact
        {
            get
            {
                return _currentContact;
            }
            set
            {
                _currentContact = value;
                OnPropertyChanged(nameof(CurrentContact));
                ((RelayCommand)SendCommand).NotifyCanExecuteChanged();
            }
        }
        public string CurrentMessage
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(CurrentMessage));
            }
        }

        public async void ExecuteSendCommand()
        {
            await _chatClient.SendMessage(new ContactInfo() { Id=CurrentContact.ID}, CurrentMessage);
            CurrentContact.Messages.Add(new MessageModel() { Text=CurrentMessage, IsSended = true, Time = DateTime.Now.ToString("HH:mm") });
            CurrentMessage = null;
        }
        public bool CanExecuteSendCommand()
        {
            return CurrentContact != null;
        }
    }
}
