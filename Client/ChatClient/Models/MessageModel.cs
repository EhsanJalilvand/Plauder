using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class MessageModel
    {
        public string Text { get; set; }
        public bool IsSended { get; set; }
        public string Time { get; set; }
    }
}
