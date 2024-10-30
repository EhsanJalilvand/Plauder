using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainShare.Settings
{
    public class ServerSetting
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public int ChunkSize { get; set; }
    }
}
