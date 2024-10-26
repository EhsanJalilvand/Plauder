using ApplicationShare.Dtos;
using ChatClient.ViewModels;
using Client.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IClientService _chatClient;
        private readonly MainViewModel _mainViewModel;
        public MainWindow(IClientService chatClient, MainViewModel mainViewModel)
        {
            InitializeComponent();
            _chatClient = chatClient;
            _mainViewModel = mainViewModel;
            _chatClient.MessageCallBack = RecieveNewMessage;
            this.DataContext = _mainViewModel;
        }
        public async void RecieveNewMessage(MessageContract messageContract)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.NotifyOnline && !_mainViewModel.Data.ContactInfos.Any(a => a.Name == messageContract.Sender))
                    _mainViewModel.Data.ContactInfos.Add(new ApplicationShare.Dtos.ContactModel() { Ip = "127.0.0.1", Name = messageContract.Sender });
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.NotifyOffline && _mainViewModel.Data.ContactInfos.Any(a => a.Name == messageContract.Sender))
                    _mainViewModel.Data.ContactInfos.Remove(_mainViewModel.Data.ContactInfos.First(a => a.Name == messageContract.Sender));
                if (messageContract.MessageType == ApplicationShare.Enums.MessageType.Message)
                {
                    var contact=_mainViewModel.Data.ContactInfos.FirstOrDefault(p=>p.Name==messageContract.Reciever);
                    contact.Messages.Add(messageContract.Message);
                }
            });
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _chatClient.RegisterClient();
        }
    }
}
