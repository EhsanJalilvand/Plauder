﻿using ApplicationShare.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationShare.Dtos
{
    public class MessageContract
    {
        public ContactInfo Sender { get; set; }
        public ContactInfo Reciever { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
    }
}
