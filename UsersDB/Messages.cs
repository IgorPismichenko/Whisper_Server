using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDB
{
    public class Messages
    {
        public int Id { get; set; }
        public string? SenderIp { get; set; }
        public string? ReceiverIp { get; set; }
        public string? Message { get; set; } = null!;
        public int RoomId { get; set; }
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime DeletedAt { get; set; }

        public virtual Room Room { get; set; } = null!;

        public virtual Users User { get; set; } = null!;
    }
}
