using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XMLReporting
{
    public class TextReplacementTemplate : TextReplacement
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="current"></param>
        public TextReplacementTemplate(string key, XElement current)
        {
            Key = key;

            List<Group> groups = new List<Group>();
            while (current != null)
            {
                if (current.Attributes("data-group").Any())
                {
                    var attribute = current.Attributes("data-group").Single();
                    groups.Insert(0, new Group(current.Parent.Attribute("data-foreach").Value, attribute.Value));
                }
                else if (current.Attributes("data-foreach").Any())
                {
                    var attribute = current.Attributes("data-foreach").Single();

                    if (!groups.Any() || attribute.Value != groups[0].Key)
                        groups.Insert(0, new Group(attribute.Value, "*"));
                }

                current = current.Parent;
            }

            Groups = groups.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        public TextReplacementTemplate Apply(IDataSource source)
        {
            object f = source[Key, Groups.ToArray()];

            return this;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Groups.Any() ? string.Format("{0}: {1}", string.Join("->", Groups), Key) : Key;
        }
    }
}
