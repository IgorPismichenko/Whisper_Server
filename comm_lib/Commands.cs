using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace comm_lib
{
    [DataContract]
    public class Commands
    {
        [DataMember]
        public string? login { get; set; }
        [DataMember]
        public string? password { get; set; }
        [DataMember]
        public string? phone { get; set; }
        [DataMember]
        public string? command { get; set; }
    }
}
