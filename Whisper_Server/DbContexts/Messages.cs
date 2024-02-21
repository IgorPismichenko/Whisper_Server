using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper_Server.DbContexts
{
    public class Messages
    {
        public int Id { get; set; }
        public string? SenderIp { get; set; }
        public string? ReceiverIp { get; set; }
        public string? Message { get; set; }
    }
}
