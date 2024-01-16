using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsersDB
{
    public partial class Room
    {
        public int Id { get; set; }

        public string? Subject { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime DeletedAt { get; set; }

        public virtual ICollection<Messages> Messages { get; set; } = new List<Messages>();

        public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
