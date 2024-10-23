using ApplicationShare.Dtos;
using ApplicationShare.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class MessageContract
    {
        public string Reciever { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
    }
}
