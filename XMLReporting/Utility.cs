using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLReporting
{
    public static class Utility
    {
        public static IEnumerable<DataRow> ToArray(this DataRowCollection collection)
        {
            foreach (DataRow dataRow in collection)
                yield return dataRow;
        }

        public static IEnumerable<DataRow> Where(this DataRowCollection collection, Func<DataRow, bool> predicate)
        {
            return collection.ToArray().Where(predicate);
        }

        public static IEnumerable<XElement> GetParentElements(this XElement element)
        {
            List<XElement> elements = new List<XElement>();

            XElement currentElement = element.Parent;
            while (currentElement != null)
            {
                elements.Add(currentElement);
                currentElement = currentElement.Parent;
            }

            return elements;
        }
    }
}
