using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Dtos
{
    public class ContactModel : INotifyPropertyChanged
    {
        private string _name;
        private string _ip;
        private readonly ObservableCollection<string> _messages = new ObservableCollection<string>();
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Ip
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
                OnPropertyChanged("Ip");
            }
        }
        public ObservableCollection<string> Messages
        {
            get { return _messages; }
        }
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

