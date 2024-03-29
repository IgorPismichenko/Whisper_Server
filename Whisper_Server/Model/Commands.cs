using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Whisper_Server.Model

{
    [Serializable]
    [DataContract]
    public class User
    {
        [DataMember]
        public string? login { get; set; }
        [DataMember]
        public string? password { get; set; }
        [DataMember]
        public string? phone { get; set; }
        [DataMember]
        public string? command { get; set; }
        [DataMember]
        public string? mess { get; set; }
        [DataMember]
        public string? path { get; set; }
        [DataMember]
        public string? contact { get; set; }
        [DataMember]
        public List<Chat>? chat { get; set; }
        [DataMember]
        public byte[]? avatar { get; set; }
        [DataMember]
        public List<Profile>? profile { get; set; }
        [DataMember]
        public List<byte[]>? mediaList { get; set; }
        [DataMember]
        public byte[]? media { get; set; }

        [DataMember]
        public string? data { get; set; }
        [DataMember]
        public Chat? c {  get; set; }
        [DataMember]
        public string? blocked { get; set; }
        [DataMember]
        public string? isOnline { get; set; }
    }
}
