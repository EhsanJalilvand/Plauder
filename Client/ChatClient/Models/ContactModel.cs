using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Dtos
{
    public class ContactModel
    {
        private string _name;
        private string _id;
        private readonly ObservableCollection<MessageModel> _messages = new ObservableCollection<MessageModel>();
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public ObservableCollection<MessageModel> Messages
        {
            get { return _messages; }
        }
    }
}

