using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System;

namespace XMLReporting
{
    [DataContract]
    public abstract class TextReplacement
    {
        /// <summary>
        /// The original identifier of the text replacement in the XML
        /// </summary>
        [DataMember]
        public string Key { get; protected set; }

        /// <summary>
        /// The logical grouping of this text replacement in the XML
        /// </summary>
        [DataMember]
        public IEnumerable<Group> Groups { get; protected set; }
    }
}
