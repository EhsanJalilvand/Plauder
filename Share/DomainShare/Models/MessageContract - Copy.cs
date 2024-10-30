using DomainShare.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainShare.Models
{
    public class MessageChunk
    {
        public Guid MessageId { get; set; }
        public string ClientId { get; set; }
        public string RecieverId { get; set; }
        public int TotalChunk { get; set; }
        public int ChunkNumber { get; set; }
        public byte[] Message { get; set; }
    }
}
