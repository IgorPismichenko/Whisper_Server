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
        public string? BlockerIp { get; set; }
        public string? BloсkedIp { get; set; }
        public bool? Value { get; set; }
    }
}
