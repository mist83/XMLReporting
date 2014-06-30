using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using MediaForge3.Common.DataContracts.TMS.Programs;

namespace ConsoleApplication1
{
    class Program
    {
        private static int indentAmount = 4;
        private static string stagingTablePrefix = "_";

        static void Main(string[] args)
        {
            int result = new AsyncExamples().MyTaskAsync().Result;

            var tableCreationStatements = GenerateSQL(typeof(programsProgram), 0, "programsProgram", "castTypeMemberName", "crewTypeMemberName").ToList();
            tableCreationStatements.Reverse();
            var dropStatements = tableCreationStatements.Where(x => !x.StartsWith("--")).Select(x => x.Substring(0, x.IndexOf(" (")).Trim().Replace("CREATE", "DROP")).ToList();

            //dropStatements.Insert(0, "/*");
            //dropStatements.Add("*/");

            string sql = string.Join(Environment.NewLine, dropStatements.Union(tableCreationStatements)) + Environment.NewLine;

            using (SqlConnection connection = new SqlConnection(new SqlConnectionStringBuilder { DataSource = "dlabtwsql121", InitialCatalog = "TMS_Test", IntegratedSecurity = true }.ToString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            //var loaded = on.Deserialize(@"C:\Users\mullman\Documents\Visual Studio 2013\Projects\ConsoleApplication1\ConsoleApplication1\on_mov_programs-up_00000009.xml");
            var loaded = on.Deserialize(File.ReadAllText(@"C:\Users\mullman\Desktop\on_mov_programs-up_20140521.xml"));

            //string allSql = sql + Environment.NewLine + insertSql + Environment.NewLine;

            DateTime startTime = DateTime.Now;
            for (int i = 0; i < loaded.programs.Count; i++)
            {
                if (i % 25 == 0)
                    Debug.WriteLine("Item {0}/{1}: Elapsed: {2}", i + 1, loaded.programs.Count, DateTime.Now.Subtract(startTime).Duration());

                var item = loaded.programs[i];

                var inserts = GenerateSQLInsertStatement(item, loaded.programs.First().TMSId, "castTypeMemberName", "crewTypeMemberName");
                string insertSql = string.Join(Environment.NewLine, inserts) + Environment.NewLine;

                using (SqlConnection connection = new SqlConnection(new SqlConnectionStringBuilder { DataSource = "dlabtwsql121", InitialCatalog = "TMS_Test", IntegratedSecurity = true }.ToString()))
                {
                    connection.Open();

                    //string[] lines = insertSql.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (string line in lines)
                    {
                        using (SqlCommand command = new SqlCommand(insertSql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            //DrawRedditAlien(width: 60);
            var a = TimeSpan.FromSeconds(12345).ToString();
            var b = TimeSpan.FromSeconds(12390).ToString("hh\\:mm\\:ss");

            byte[] bytes1 = new SHA1CryptoServiceProvider().ComputeHash("input1".Select(x => (byte)x).ToArray());
            string hex1 = string.Join(string.Empty, bytes1.Select(x => string.Format("{0:x2}", x)));

            byte[] bytes2 = new SHA1CryptoServiceProvider().ComputeHash("input2".Select(x => (byte)x).ToArray());
            string hex2 = string.Join(string.Empty, bytes2.Select(x => string.Format("{0:x2}", x)));

            CountLines(args.First(), args.Skip(1).ToArray());

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
        }

        public static HashSet<string> GenerateSQL(Type type, int indentLevel, params string[] nonLinkedTableNames)
        {
            HashSet<string> sql = new HashSet<string>();
            var properties = type.GetProperties().OrderBy(x => x.Name).Reverse();

            var classProperties = properties.Where(x =>
                x.PropertyType != typeof(string) &&
                x.PropertyType != typeof(decimal) &&
                x.PropertyType != typeof(DateTime) &&
                x.PropertyType != typeof(bool) &&
                x.PropertyType != typeof(float));

            List<PropertyInfo> childLinkingProperties = new List<PropertyInfo>();
            if (classProperties.Any())
            {
                foreach (var item in classProperties)
                {
                    // Add properties that will be picked up when the table is generated that links this record to the one below
                    childLinkingProperties.AddRange(item.PropertyType.GetProperties().Where(x => x.Name.EndsWith("Id")));

                    var subProperties = GenerateSQL(item.PropertyType.GenericTypeArguments.Any() ? item.PropertyType.GenericTypeArguments.Single() : item.PropertyType, indentLevel + 1, nonLinkedTableNames);
                    foreach (var subTable in subProperties)
                    {
                        if (sql.Any(x => x.Trim() == subTable.Trim()))
                            sql.Add("--" + subTable);
                        else
                            sql.Add(subTable);
                    }
                }
            }

            var simpleProperties = properties.Union(childLinkingProperties).Except(classProperties).Where(x => !x.PropertyType.GenericTypeArguments.Any());
            if (simpleProperties.Any())
            {
                if (nonLinkedTableNames.Contains(type.Name))
                {
                    var sqlStatement = string.Format("{0}CREATE TABLE [dbo].[{1}{2}] ({3})", string.Empty.PadLeft(indentLevel * indentAmount, ' '), stagingTablePrefix, type.Name, string.Join(", ",
                        simpleProperties
                        .OrderBy(x => x.Name == "TMSId" ? 0 : 1)
                        .ThenBy(x => x.Name.EndsWith("Id") ? 0 : 1)
                        .ThenBy(x => x.Name)
                        .Select(x => string.Format("[{0}] {1}", x.Name, ConvertTypeToSQLType(x.PropertyType)))));

                    sql.Add(sqlStatement);
                }
                else
                {
                    string sqlStatement = string.Format("{0}CREATE TABLE [dbo].[{1}{2}] ([TMSId] varchar(max), {3})", string.Empty.PadLeft(indentLevel * indentAmount, ' '), stagingTablePrefix, type.Name, string.Join(", ",
                        simpleProperties
                        .OrderBy(x => x.Name.EndsWith("Id") ? 0 : 1)
                        .Select(x => string.Format("[{0}] {1}", x.Name, ConvertTypeToSQLType(x.PropertyType)))));

                    sql.Add(sqlStatement);
                }
            }

            return sql;
        }

        static string ConvertTypeToSQLType(Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(string))
                return "nvarchar(max)";
            else if (type == typeof(DateTime))
                return "datetime";
            else if (type == typeof(bool))
                return "bit";
            else if (type == typeof(Single))
                return "float";
            else if (type == typeof(Guid))
                return "uniqueidentifier";
            else
                throw new NotImplementedException();
        }

        public static IEnumerable<string> GenerateSQLInsertStatement(object item, string tmsId, params string[] excludedTables)
        {
            List<string> statements = new List<string>();

            if (item is string)
                return statements;

            var simplePropertyInfos = GetSimplePropertyInfos(item).ToList();
            List<PropertyInfo> linkingProperties = new List<PropertyInfo>();
            foreach (var classProperty in item.GetType().GetProperties().Except(simplePropertyInfos))
            {
                // Add properties that will be picked up when the table is generated that links this record to the one below
                linkingProperties.AddRange(classProperty.PropertyType.GetProperties().Where(x => x.Name.EndsWith("Id")));

                if (!classProperty.PropertyType.GenericTypeArguments.Any())
                    statements.AddRange(GenerateSQLInsertStatement(classProperty.GetValue(item), tmsId, excludedTables));
                else
                {
                    var enumerable = (IEnumerable)classProperty.GetValue(item);
                    foreach (var listItem in enumerable)
                    {
                        statements.AddRange(GenerateSQLInsertStatement(listItem, tmsId, excludedTables));
                    }
                }
            }

            if (simplePropertyInfos.Any())
            {
                if (item.GetType().Name == "awardType")
                {

                }
                if (!excludedTables.Contains(item.GetType().Name))
                {
                    if (linkingProperties.Any())
                    {
                        string sql = string.Format("INSERT {0}{1}(" + (item.GetType() == typeof(programsProgram) ? string.Empty : "[TMSId], ") + "{2}, {3}) VALUES({4} {5}, {6})",
                            stagingTablePrefix,
                            item.GetType().Name,
                            string.Join(", ", linkingProperties.Select(x => string.Format("[{0}]", x.Name))),
                            string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))),
                            (item.GetType() == typeof(programsProgram) ? string.Empty : "'" + tmsId + "', "),
                            string.Join(", ", Enumerable.Repeat("'unknown'", linkingProperties.Count)),
                            GetSqlPropertyValues(item));
                        statements.Add(sql);
                    }
                    else
                    {
                        string sql = string.Format("INSERT {0}{1}([TMSId], {2}) VALUES('{3}', {4})",
                            stagingTablePrefix,
                            item.GetType().Name,
                            string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))),
                            tmsId,
                            GetSqlPropertyValues(item));
                        statements.Add(sql);
                    }
                }
                else
                {
                    string sql = string.Format("INSERT {0}{1}({2}) VALUES({3})", stagingTablePrefix, item.GetType().Name, string.Join(", ", simplePropertyInfos.Select(x => string.Format("[{0}]", x.Name))), GetSqlPropertyValues(item));
                    statements.Add(sql);
                }
            }

            return statements;
        }

        public static IEnumerable<PropertyInfo> GetSimplePropertyInfos(object item)
        {
            var simpleProperties = item.GetType().GetProperties()
                .OrderBy(x => x.Name)
                .Where(x =>
                    x.PropertyType == typeof(int) ||
                    x.PropertyType == typeof(decimal) ||
                    x.PropertyType == typeof(string) ||
                    x.PropertyType == typeof(DateTime) ||
                    x.PropertyType == typeof(bool) ||
                    x.PropertyType == typeof(Single) ||
                    x.PropertyType == typeof(Guid) ||
                    x.PropertyType == typeof(float));

            return simpleProperties;
        }

        public static string GetSqlPropertyValues(object item)
        {
            return string.Join(", ", GetSimplePropertyInfos(item).Select(x => GetSQLPropertyValue(item, x)));
        }

        public static string GetSQLPropertyValue(object item, PropertyInfo info)
        {
            string sqlProperty = "NULL";

            if (info.PropertyType == typeof(int))
                sqlProperty = info.GetValue(item).ToString();
            if (info.PropertyType == typeof(decimal))
                sqlProperty = info.GetValue(item).ToString();
            else if (info.PropertyType == typeof(string))
                sqlProperty = string.Format("'{0}'", ((string)info.GetValue(item) ?? string.Empty).Replace("'", "''"));
            else if (info.PropertyType == typeof(DateTime))
                sqlProperty = string.Format("'{0}'", DateTime.Parse(info.GetValue(item).ToString()) < new DateTime(1753, 1, 1) ? new DateTime(1753, 1, 1) : DateTime.Parse(info.GetValue(item).ToString()));
            else if (info.PropertyType == typeof(bool))
                sqlProperty = (bool)info.GetValue(item) ? "1" : "0";
            else if (info.PropertyType == typeof(Single))
                sqlProperty = info.GetValue(item).ToString();
            else if (info.PropertyType == typeof(Guid))
                sqlProperty = string.Format("'{0}'", info.GetValue(item));

            if (sqlProperty == "''")
                sqlProperty = "NULL";

            return sqlProperty;
        }

        private static void TestAbort()
        {
            Thread t = new Thread(new ThreadStart(abortme));
            t.Start();
            Thread.Sleep(200);
            t.Abort();
            Thread.Sleep(1000);

            Console.Write("Press any key to continue . . .");
            Console.ReadKey();
            return;
        }

        private static void abortme()
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Thread was aborted.");
                try
                {
                    return;
                }
                catch
                {
                    Console.WriteLine("Can't even call 'return' on aborted thread");
                }
            }
        }

        #region Draw the Reddit Alien

        static void DrawRedditAlien(string textToUse = "REDDIT*", int? width = null)
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

            if (!width.HasValue)
                width = Console.BufferWidth;

            double resizeFactor = (double)width / (double)bitmap.Width;
            Size newSize = new Size((int)((double)bitmap.Width * resizeFactor) - 1, (int)((double)bitmap.Width * resizeFactor * .8) - 1);
            bitmap = new Bitmap(bitmap, newSize);

            int characterIndex = 0;

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

                        Console.Write(textToUse[characterIndex % textToUse.Length]);
                        characterIndex++;
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
    }
}
