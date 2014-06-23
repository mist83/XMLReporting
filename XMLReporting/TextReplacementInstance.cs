using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace XMLReporting
{
    [DataContract]
    public class TextReplacementInstance : TextReplacement
    {
        public TextReplacementInstance(string key, string value, params Group[] groups)
        {
            Key = key;
            Value = value;
            Groups = groups;
        }

        [DataMember]
        public string Value { get; private set; }

        public override string ToString()
        {
            return Groups.Any() ? string.Format("{0} -> {1}", string.Join(" ---> ", (IEnumerable<Group>)Groups), string.Format("{0}: {1}", Key, Value)) : string.Format("{0}: {1}", Key, Value);
        }
    }
}
