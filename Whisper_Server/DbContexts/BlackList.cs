using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper_Server.DbContexts
{
    public class BlackList
    {
        public int Id { get; set; }
        public bool Value { get; set; }
        public virtual Users? BlockedUser { get; set; }
        public virtual Users? BlockerUser { get; set; }

        public int BlockerUserId { get; set; }

        public int BlockedUserId { get; set; }
    }
}
