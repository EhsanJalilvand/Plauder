using Application.Dtos;
using ApplicationShare.Dtos;
using ChatClient.Models;
using Client;
using Client.Application.Services;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainScreen _mainScreen;
        private readonly IClientService _chatClient;
        private ContactModel _currentContact;
        private string _message;
        public event PropertyChangedEventHandler? PropertyChanged;

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
            await _chatClient.SendMessage(CurrentContact.Name, CurrentMessage);
            CurrentContact.Messages.Add(CurrentMessage);
            CurrentMessage = string.Empty;
        }
        public bool CanExecuteSendCommand()
        {
            return CurrentContact != null;
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
