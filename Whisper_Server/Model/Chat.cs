using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Whisper_Server.Model
{
    [Serializable]
    [DataContract]
    public class Chat
    {
        [DataMember]
        public string? chatContact { get; set; }
        [DataMember]
        public string? date { get; set; }
        [DataMember]
        public string? message { get; set; }
        [DataMember]
        public byte[]? media { get; set; }
    }
}
