﻿using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace comm_lib
{
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
        public string? contact { get; set; }
        [DataMember]
        public List<string>? chat { get; set; }
        [DataMember]
        public byte[] avatar { get; set; }
        [DataMember]
        public List<User>? contactList { get; set; }
    }
}
