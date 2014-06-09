using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLReporting
{
    public class TextReplacement
    {
        private List<string> _groups = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="current"></param>
        public TextReplacement(string key, XElement current)
        {
            Key = key;
            while (current != null)
            {
                if (current.Attributes("data-foreach").Any())
                {
                    var attribute = current.Attributes("data-foreach").Single();

                    if (!_groups.Any() || attribute.Value != _groups[0])
                        _groups.Insert(0, attribute.Value);
                }

                current = current.Parent;
            }
        }

        /// <summary>
        /// The original identifier of the text replacement in the XML
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The logical grouping of this text replacement in the XML
        /// </summary>
        public IEnumerable<string> Groups
        {
            get
            {
                foreach (var item in _groups)
                    yield return item;
            }
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
