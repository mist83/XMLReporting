using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XMLReporting
{
    public static class TextReplacementUtility
    {
        public static IEnumerable<TextReplacement> GetTextReplacements(string originalXML, string textReplacementRegex)
        {
            var textReplacements = new List<TextReplacement>();

            string xml = originalXML;

            // Nuke any invalid tags (for example, scripts that have invalid XML characters in them)
            MatchCollection collection = Regex.Matches(xml, @"<script>(.|\n)*</script>");
            Dictionary<string, Guid> replacements = new Dictionary<string, Guid>();
            foreach (Match item in collection)
                xml = xml.Replace(item.Value, Guid.NewGuid().ToString());

            var xmlElement = XElement.Parse(xml);

            IEnumerable<XNode> nodes = xmlElement.DescendantNodesAndSelf();
            foreach (var item in nodes)
            {
                if (item is XElement)
                {
                    var element = (XElement)item;
                    if (element.HasAttributes)
                    {
                        foreach (var attribute in element.Attributes())
                        {
                            MatchCollection replacementMatches = Regex.Matches(attribute.Value, textReplacementRegex);
                            foreach (Match match in replacementMatches)
                            {
                                var textReplacement = new TextReplacement(match.Value, element.Parent);
                                textReplacements.Add(textReplacement);
                            }
                        }
                    }
                }
                else if (item is XText)
                {
                    var text = (XText)item;
                    MatchCollection replacementMatches = Regex.Matches(text.Value, textReplacementRegex);
                    foreach (Match match in replacementMatches)
                    {
                        var textReplacement = new TextReplacement(match.Value, text.Parent);
                        textReplacements.Add(textReplacement);
                    }
                }

                // Note: comments do not have their content replaced
            }

            return textReplacements;
        }

        public static string Apply(string originalXML, IDataSource source, string textReplacementRegex)
        {
            string xml = originalXML;
            MatchCollection collection = Regex.Matches(xml, @"<script>(.|\n)*</script>");

            Dictionary<string, Guid> replacements = new Dictionary<string, Guid>();
            foreach (Match item in collection)
                replacements[item.Value] = Guid.NewGuid();
            foreach (var item in replacements)
                xml = xml.Replace(item.Key, item.Value.ToString());

            var textReplacements = TextReplacementUtility.GetTextReplacements(xml, textReplacementRegex);
            var groups = textReplacements.SelectMany(x => x.Groups).Distinct();

            XElement element = XElement.Parse(xml);

            IEnumerable<XElement> topLevelGroupingElements = GetTopLevelGroupingElements(element, true);
            foreach (XElement topLevel in topLevelGroupingElements)
            {
                var grouping = GetGrouping(topLevel).ToArray();
                ApplyGroups(topLevel, source, grouping);
            }

            // re-add the scripts
            string final = element.ToString();
            foreach (var item in replacements)
                final = final.Replace(item.Value.ToString(), item.Key);

            //var result1 = table[textReplacements.Skip(2).First().Key, "g1", "g2 value 0"];
            //var result2 = table[textReplacements.Skip(4).First().Key, "g1", "g2 value 2", "g3"];

            F();

            return final;
        }

        [DataContract]
        public class ReportData
        {
            public ReportData(string key, string value, params Group[] groups)
            {
                Key = key;
                Value = value;
                Groups = groups;
            }

            [DataMember]
            public Group[] Groups { get; private set; }

            [DataMember]
            public string Key { get; private set; }

            [DataMember]
            public string Value { get; private set; }

            public override string ToString()
            {
                return Groups.Any() ? string.Format("{0} -> {1}", string.Join(" ---> ", (IEnumerable<Group>)Groups), string.Format("{0}: {1}", Key, Value)) : string.Format("{0}: {1}", Key, Value);
            }
        }

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

        private static void F()
        {
            ReportData serializedData = new ReportData("Key", Guid.NewGuid().ToString(), new Group("GROUP1", "g1 value"), new Group("GROUP2", "g2 value"));

            var s = new DataContractJsonSerializer(typeof(ReportData));
            using (FileStream stream = new FileStream(@"C:\Temp\out.json", FileMode.Create))
                s.WriteObject(stream, serializedData);

            using (FileStream stream = new FileStream(@"C:\Temp\out.json", FileMode.Open))
            {
                ReportData deserializedData = (ReportData)s.ReadObject(stream);
            }
        }

        private static IEnumerable<XElement> GetTopLevelGroupingElements(XElement element, bool includeSelf = false)
        {
            List<XElement> topLevelGroupingElements = element.DescendantsAndSelf().Where(x => x.Attributes("data-foreach").Count() == 1).ToList();
            if (!includeSelf && topLevelGroupingElements.Contains(element))
                topLevelGroupingElements.Remove(element);

            topLevelGroupingElements.RemoveAll(x => x.GetParentElements().Intersect(topLevelGroupingElements).Any());

            return topLevelGroupingElements;
        }

        private static void ApplyGroups(XElement element, IDataSource source, params Tuple<string, string>[] parentGroups)
        {
            var contents = string.Join(Environment.NewLine, element.Nodes().Select(x => x.ToString()));
            element.RemoveNodes();

            var uniqueGroups = source.GetUniqueGroups(parentGroups);
            foreach (string group in uniqueGroups)
            {
                string name = element.Attribute("data-foreach").Value;

                XElement contentElement = XElement.Parse(string.Format("<grouping key=\"{0}\">{1}{2}{1}</grouping>", group, Environment.NewLine, contents));
                element.Add(contentElement);

                IEnumerable<XElement> children = GetTopLevelGroupingElements(contentElement);
                foreach (XElement child in children)
                {
                    var childGrouping = GetGrouping(child);
                    ApplyGroups(child, source, childGrouping.ToArray());
                }
            }
        }

        private static IEnumerable<Tuple<string, string>> GetGrouping(XElement element)
        {
            List<Tuple<string, string>> groupings = new List<Tuple<string, string>>();

            // Find the foreach attribute (if any)
            XElement current = element;
            while (current != null)
            {
                if (current.Attributes("data-foreach").Any())
                {
                    var grouping = Tuple.Create(current.Attribute("data-foreach").Value, "*");
                    groupings.Add(grouping);
                    break;
                }

                current = current.Parent;
            }

            // Find the current group (if any)
            current = element;
            while (current != null)
            {
                if (current.Name.LocalName == "grouping")
                {
                    var grouping = Tuple.Create(current.Parent.Attribute("data-foreach").Value, current.Attribute("key").Value);
                    groupings.Insert(0, grouping);
                }

                current = current.Parent;
            }

            return groupings;
        }

        public static IDataSource BuildMockDataSource(this IEnumerable<TextReplacement> textReplacements)
        {
            DataTable table = new DataTable();

            HashSet<string> groups = new HashSet<string>();
            HashSet<string> columns = new HashSet<string>();
            foreach (var textReplacement in textReplacements)
            {
                columns.Add(textReplacement.Key);
                foreach (var group in textReplacement.Groups)
                    groups.Add(group);
            }

            table.Columns.Add("Id", typeof(Guid));
            table.Columns["Id"].Unique = true;

            // The ID of the main report item
            table.Columns.Add("MainReportItemId", typeof(Guid));

            foreach (var group in groups.Distinct().OrderBy(x => x))
                table.Columns.Add(group, typeof(string));

            foreach (var column in columns.Distinct().OrderBy(x => x))
                table.Columns.Add(column.Trim('{', '}'), typeof(string));

            // Fill with mock data
            Guid mainItemID = Guid.NewGuid();
            DataRow newRow = table.NewRow();
            newRow["Id"] = Guid.NewGuid();
            newRow["MainReportItemID"] = mainItemID;
            newRow["group1"] = "g1";
            newRow["group2"] = "g2 value 0";
            newRow["Column_D"] = "D0";
            table.Rows.Add(newRow);

            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1", "g2 value 1", "g3 v1", "A1", "B1", "C1", "D1", "E2");
            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1", "g2 value 1", "g3 v2", "A1", "B1", "C1", "D1", "E2");
            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1", "g2 value 1", "g3 v3", "A1", "B1", "C1", "D1", "E2");

            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1", "g2 value 2", "g3", "A2", "B2", "C2", "D2", "E2 value");
            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1a", "g2 value 2", "g3", "A2", "B2", "C2", "D2", "E2 value");
            table.Rows.Add(Guid.NewGuid(), mainItemID, "g1a", "g2 value 3", "g3", "A2", "B2", "C2", "D2", "E2 value");

            return new DataTableDataSource(table);
        }
    }
}
