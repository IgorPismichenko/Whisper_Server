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
        public int UserId { get; set; }
        public string TextMessage { get; set; }
    }
}
