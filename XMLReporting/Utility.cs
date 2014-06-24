using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
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

        public static Stream ToStream(this string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

        public static string ContentToString(this Stream stream)
        {
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        public static T ReadObject<T>(this DataContractJsonSerializer serializer, Stream stream)
        {
            return (T)serializer.ReadObject(stream);
        }
    }
}
