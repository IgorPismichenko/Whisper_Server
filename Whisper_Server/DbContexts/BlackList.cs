using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [ForeignKey("BlockedUser")]
        public int BlockedUserId { get; set; }

        [ForeignKey("BlockerUser")]
        public int BlockerUserId { get; set; }
    }
}
