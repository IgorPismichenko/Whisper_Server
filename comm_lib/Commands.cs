using System.Collections.ObjectModel;
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
        public ObservableCollection<string>? contacts { get; set; }
    }
    public class Chat
    {
        [DataMember]
        public ObservableCollection<string>? messages { get; set; }
        [DataMember]
        public string? contact { get; set; }
        [DataMember]
        public string? mess { get; set; }
    }
}
