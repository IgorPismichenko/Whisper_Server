using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper_Server.DbContexts
{
    public class Messages
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? Date { get; set; }
        public string? Media { get; set;}
        public virtual Users? SenderUser { get; set; }
        public virtual Users? ReceiverUser { get; set; }

        [ForeignKey("SenderUser")]
        public int SenderUserId { get; set; }

        [ForeignKey("ReceiverUser")]
        public int ReceiverUserId { get; set; }
    }
}
