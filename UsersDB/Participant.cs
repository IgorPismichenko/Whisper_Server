using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDB
{
    public partial class Participant
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int UserId { get; set; }

        public DateTime LastRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime DeleteedAt { get; set; }

        public virtual Room Room { get; set; } = null!;

        public virtual Users User { get; set; } = null!;
    }
}
