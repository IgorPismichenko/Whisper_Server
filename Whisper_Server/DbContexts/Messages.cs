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
        //public string? SenderLogin { get; set; }
        //public string? ReceiverLogin { get; set; }
        public string? Message { get; set; }
        public string? Date { get; set; }
        public string? Media { get; set;}
        public virtual Users? SenderUser { get; set; }

        public int SenderUserId { get; set; }
        public virtual Users? ReceiverUser { get; set; }

        public int ReceiverUserId { get; set; }
    }
}
