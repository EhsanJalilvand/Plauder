using ApplicationShare.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class MainScreen : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public MainScreen()
        {
            ContactInfos = new ObservableCollection<ContactModel>();
        }

        public ObservableCollection<ContactModel> ContactInfos { get; set; }

    }
}
