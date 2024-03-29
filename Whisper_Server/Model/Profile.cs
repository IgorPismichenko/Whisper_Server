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
    public class Profile
    {
        [DataMember]
        public string? login { get; set; }
        [DataMember]
        public byte[]? avatar { get; set; }
        [DataMember]
        public string? phone { get; set; }
    }
}
