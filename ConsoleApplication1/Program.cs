using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var textReplacements = GetTextReplacements(xml);
            var table = textReplacements.BuildMockTable();
            Apply(xml, table);

            //DrawAlien();

            Console.WriteLine();

            CountLines(args.First(), args.Skip(1).ToArray());

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
        }

        #region draw alien

        static void DrawAlien()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            var bitmap = (Bitmap)GetImageFromUrl(@"http://upload.wikimedia.org/wikipedia/fr/f/fc/Reddit-alien.png");

            var rectangle = GetAlphaBoundingRect(bitmap);
            var bm = new Bitmap(rectangle.Width, rectangle.Height);

            // Copy the bits from the old image to the new one
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var color = bitmap.GetPixel(x + rectangle.Left, y + rectangle.Top);
                    bm.SetPixel(x, y, color);
                }
            }

            bitmap = bm;

            double resizeFactor = (double)Console.BufferWidth / (double)bitmap.Width;
            Size newSize = new Size((int)((double)bitmap.Width * resizeFactor) - 1, (int)((double)bitmap.Width * resizeFactor * .8) - 1);
            bitmap = new Bitmap(bitmap, newSize);

            string textToUse = "REDDIT*";
            int i = 0;

            var colors = new HashSet<Color>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    ConsoleColor? overrideColor = null;

                    // Eyes
                    if (color.ToString() == "Color [A=255, R=255, G=86, B=0]")
                        overrideColor = ConsoleColor.Red;

                    // Outline
                    if (color.A != 0 && color.R == 0 && color.G == 0 && color.B == 0)
                        overrideColor = ConsoleColor.DarkGray;

                    // Track colors
                    if (color.A != 0 && (color.R != 0 || color.G != 0 || color.B != 0))
                        colors.Add(color);

                    // Show the color as white or one of the predefined colors
                    if (overrideColor.HasValue || (color.R != 0 && color.G != 0 && color.B != 0))
                    {
                        ConsoleColor defaultColor = Console.ForegroundColor;

                        if (overrideColor.HasValue)
                            Console.ForegroundColor = overrideColor.Value;

                        Console.Write(textToUse[i % textToUse.Length]);
                        i++;
                        Console.ForegroundColor = defaultColor;
                    }
                    else
                        Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private static Rectangle GetAlphaBoundingRect(Bitmap bitmap)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int width = int.MinValue;
            int height = int.MinValue;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (x < left)
                            left = x;

                        if (y < top)
                            top = y;

                        if (x > width)
                            width = x;

                        if (y > height)
                            height = y;
                    }
                }
            }

            return new Rectangle(left, top, width - left, height - top);
        }

        #endregion

        #region count lines

        static void CountLines(string directory, params string[] fileTypes)
        {
            var initialColor = Console.ForegroundColor;

            var total = 0;
            var totalFiles = 0;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Code line count for: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(directory);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");
            foreach (var item in fileTypes)
            {
                var files = Directory.GetFiles(directory, "*." + item, SearchOption.AllDirectories);
                totalFiles += files.Length;

                var codeLines = files.Sum(x => File.ReadAllLines(x).Length);
                total += codeLines;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("|");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("*.{0}", item.ToString().PadRight(10));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(string.Format("{0,4} file(s)", files.Length).PadRight(12));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("{0,16:n0} line(s)", codeLines);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("|");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("|");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("{0}", "total".PadRight(12));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(string.Format("{0,4} file(s)", totalFiles).PadRight(12));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0,16:n0} line(s)", total);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("|");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = initialColor;
        }

        #endregion

        #region XML reporting

        #region template

        private static string xml =
        @"
<html>
    <head>
        <script>
            if(1 < 2)
                document.write('The < sign will normally cause xml parsing to fail, but we remove the entire script block with a regex');
        </script>
    </head>
    <body>
        <div>
            <span>Some content</span>
            <span>{Column_A}</span>
            <span>{Column_B}</span>
            <div data-foreach=""group1"">
                <div data-foreach=""group2"">
                    <span>{Column_D}</span>
                    <div data-foreach=""group3"" attribute=""{Column_E}"">
                        <span>{Column_C}, {Column_D}</span>
                    </div>
                </div>
            </div>
            <div data-foreach=""group4"">
                <span>{Column_F}</span>
            </div>
        </div>
    </body>
</html>";

        #endregion

        private static IEnumerable<TextReplacement> GetTextReplacements(string originalXML)
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
                            MatchCollection replacementMatches = Regex.Matches(attribute.Value, @"\{Column_[^\}]+\}");
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
                    MatchCollection replacementMatches = Regex.Matches(text.Value, @"\{Column_[^\}]+\}");
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

        private static void Apply(string originalXML, DataTable table)
        {
            string xml = originalXML;
            MatchCollection collection = Regex.Matches(xml, @"<script>(.|\n)*</script>");

            Dictionary<string, Guid> replacements = new Dictionary<string, Guid>();
            foreach (Match item in collection)
                replacements[item.Value] = Guid.NewGuid();
            foreach (var item in replacements)
                xml = xml.Replace(item.Key, item.Value.ToString());


            var textReplacements = GetTextReplacements(xml);
            var groups = textReplacements.SelectMany(x => x.Groups).Distinct();

            XElement element = XElement.Parse(xml);
            ApplyGroups(element, table);

            // re-add the scripts
            string final = element.ToString();
            foreach (var item in replacements)
                final = final.Replace(item.Value.ToString(), item.Key);

            var result1 = textReplacements.Skip(2).First().GetResult(table, "g1", "g2 value 0");
            var result2 = textReplacements.Skip(4).First().GetResult(table, "g1", "g2 value 2", "g3");
        }

        private static void ApplyGroups(XElement element, DataTable table)
        {
            List<XElement> topLevelGroupingElements = element.DescendantsAndSelf().Where(x => x.Attributes("data-foreach").Count() == 1).ToList();
            topLevelGroupingElements.RemoveAll(x => x.GetParentElements().Intersect(topLevelGroupingElements).Any());

            var g = GetGrouping(topLevelGroupingElements[0]);

            var contents = string.Join(Environment.NewLine, topLevelGroupingElements[0].Nodes().Select(x => x.ToString()));
            topLevelGroupingElements[0].RemoveNodes();
            foreach (string group in table.Rows.ToArray().Select(x => x[g.First()]).Distinct())
            {
                string name = topLevelGroupingElements[0].Attribute("data-foreach").Value;

                XElement contentElement = new XElement("grouping");
                contentElement.Add(new XAttribute("key", group));
                contentElement.Add(XElement.Parse(contents));

                topLevelGroupingElements[0].Add(contentElement);
            }
        }

        private static IEnumerable<string> GetGrouping(XElement element)
        {
            List<string> groupings = new List<string>();

            XElement current = element;
            while (current != null)
            {
                if (current.Attributes("data-foreach").Any())
                    groupings.Insert(0, current.Attribute("data-foreach").Value);

                current = current.Parent;
            }

            return groupings;
        }

        private static void ApplyGroups(XElement element, params string[] groups)
        {
            foreach (string group in groups)
            {
                XElement[] elemsNeedingGrouping = element.DescendantsAndSelf().Where(x => x.HasAttributes && x.Attributes("data-foreach").Any() && x.Attributes("data-foreach").Single().Value == group).ToArray();
                foreach (XElement elem in elemsNeedingGrouping)
                {
                    for (int i = 0; i < elemsNeedingGrouping.Length; i++)
                    {
                        var currentElement = elemsNeedingGrouping[i];
                        List<string> currentGroups = new List<string> { currentElement.Attribute("data-foreach").Value };

                        var parentsWithGroups = currentElement.GetParentElements().Where(x => x.HasAttributes && x.Attributes("data-foreach").Any());
                        if (parentsWithGroups.Any())
                            currentGroups.AddRange(parentsWithGroups.Select(x => x.Attribute("data-foreach").Value));

                        currentGroups.Reverse();
                    }
                }
            }
        }

        #endregion
    }

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

        public string GetResult(DataTable table, params string[] groupValues)
        {
            string[] groupArray = Groups.ToArray();
            if (groupValues.Length != groupArray.Length)
                throw new ArgumentException("Must maintain 1:1 match for group values");

            IEnumerable<DataRow> rows = table.Rows.ToArray();
            for (int i = 0; i < groupArray.Length; i++)
                rows = rows.Where(x => object.Equals(x[groupArray[i]], groupValues[i])).ToArray();

            return rows.Single()[Key.Trim('{', '}')].ToString();
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

    public static class Utility
    {
        public static IEnumerable<DataRow> ToArray(this DataRowCollection collection)
        {
            foreach (DataRow dataRow in collection)
                yield return dataRow;
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

        public static DataTable BuildMockTable(this IEnumerable<TextReplacement> textReplacements)
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

            foreach (var group in groups.Distinct().OrderBy(x => x))
                table.Columns.Add(group, typeof(string));

            foreach (var column in columns.Distinct().OrderBy(x => x))
                table.Columns.Add(column.Trim('{', '}'), typeof(string));

            // Fill with mock data
            DataRow newRow = table.NewRow();
            newRow["Id"] = Guid.NewGuid();
            newRow["group1"] = "g1";
            newRow["group2"] = "g2 value 0";
            newRow["Column_D"] = "D0";
            table.Rows.Add(newRow);

            table.Rows.Add(Guid.NewGuid(), "g1", "g2 value 1", "g3", "A1", "B1", "C1", "D1", "E2");
            table.Rows.Add(Guid.NewGuid(), "g1", "g2 value 2", "g3", "A2", "B2", "C2", "D2", "E2 value");
            table.Rows.Add(Guid.NewGuid(), "g1a", "g2 value 2", "g3", "A2", "B2", "C2", "D2", "E2 value");

            return table;
        }
    }
}
