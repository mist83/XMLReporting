using System.Runtime.Serialization;

namespace XMLReporting
{
    [DataContract]
    public class NodeResult
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public object Value { get; set; }
    }
}
