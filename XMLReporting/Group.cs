using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XMLReporting
{
    [DataContract]
    public class Group
    {
        public Group(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Key, Value);
        }
    }
}
