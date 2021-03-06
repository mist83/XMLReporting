﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Engine;
using MediaForge3.Common.DataContracts.TMS.Programs;

namespace ConsoleApplication1
{
    public class MyType
    {
    }

    public class _
    {
        private static ManualResetEventSlim slim = new ManualResetEventSlim(false);

        private static int upperBound = 20;
        private static int completed = 0;

        

        static void Main(string[] args)
        {
            var type = Type.GetType("MyType");
            var type2 = Type.GetType("ConsoleApplication1.MyType");
            var type3 = Type.GetType("ConsoleApplication1.MyType, ConsoleApplication1");
            
            var typeName = typeof(MyType).AssemblyQualifiedName;

            int loopI = 0;
            for (loopI = 0; loopI < 20; loopI++)
            {
                if (loopI == 10)
                {
                    goto OutsideCondition;
                }
            }

            OutsideCondition:
            {

                List<int> myValues = new List<int>(new int[] { 5, 9, 3, 4, 7, 12, 0, 15 });
                List<int> newOnly = new List<int>(new int[] { 5, 7, 1, 11, 7, 19, 76, 18 });
                List<int> newValues = new List<int>();

                for (int i = 0; i < myValues.Count; i++)
                {
                    for (int x = 0; x < newOnly.Count; x++)
                    {
                        if (!myValues.Contains(newOnly[x]))
                        {
                            newValues.Add(newOnly[x]);
                        }
                    }
                }
            }
            UseSQLCE();

            return;

            #region hide

            TestDataTableAccess();

            GoString();
            //EventLogTraceListener e = new EventLogTraceListener()
            //DelimitedListTraceListener d= new DelimitedListTraceListener("") ;
            //d.TraceOutputOptions = TraceOptions.ThreadId
            //TraceSource ts = new TraceSource()

            Stopwatch sw2 = Stopwatch.StartNew();
            for (int i = 1; i <= upperBound; i++)
            {
                int j = i;

                //Task.Run(() =>
                //    {
                //        Thread t = new Thread(() =>
                //            {
                using (var engine = new ReportingEngine())
                {
                    string outputFile = string.Format(@"C:\Users\mullman\Desktop\Output\XML2PDF {0:00}.pdf", j);

                    List<string> inputs = new List<string>();
                    inputs.Add(@"C:\Users\mullman\Documents\Visual Studio 2013\Projects\ConsoleApplication1\XML Page 1.html");
                    inputs.Add(@"C:\Users\mullman\Documents\Visual Studio 2013\Projects\ConsoleApplication1\XML Page 2.html");

                    engine.RenderXMLAsPDF(outputFile, inputs.ToArray());
                }

                Console.WriteLine("Completed " + j);

                Interlocked.Increment(ref completed);
                if (completed == upperBound)
                {
                    slim.Set();
                }
                //        });
                //    t.Start();
                //});
            }

            slim.Wait();
            sw2.Stop();

            #endregion
        }

        private static void UseSQLCE(string defaultDatabase = "XMLReporting.sdf", string defaultPassword = "password")
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (var store = new SQLCEStore(defaultDatabase, defaultPassword, true))
            {
                store.ExecuteSQL("CREATE TABLE Grouping([Key] nvarchar(4000), Value nvarchar(4000), [Order] int, ResultId uniqueidentifier)");
                store.ExecuteSQL("CREATE TABLE Result(Id uniqueidentifier, EntityId uniqueidentifier, [Key] nvarchar(4000), Type nvarchar(12), Value nvarchar(4000), Formatting nvarchar(255))");
                Debug.WriteLine("Creating database/table: {0}", sw.Elapsed);
            }

            sw.Reset();
            sw.Start();

            using (var store = new SQLCEStore(defaultDatabase, defaultPassword))
            {
                int totalItem = 0;

                int entityCount = 50;
                int resultsPerEntity = 5;
                int groupsPerResult = 3;

                for (int entityIndex = 0; entityIndex < entityCount; entityIndex++)
                {
                    Guid entityId = Guid.NewGuid();
                    for (int resultIndex = 0; resultIndex < resultsPerEntity; resultIndex++)
                    {
                        for (int groupIndex = 0; groupIndex < groupsPerResult; groupIndex++)
                        {
                            Guid resultId = Guid.NewGuid();

                            store.ExecuteSQL("INSERT INTO Grouping([Key], Value, [Order], ResultId) VALUES (@key, @value, @order, @resultId)",
                                new KeyValuePair<string, object>("key", string.Format("Group {0} Key", groupIndex + 1)),
                                new KeyValuePair<string, object>("value", string.Format("Group {0} Value", groupIndex + 1)),
                                new KeyValuePair<string, object>("order", groupIndex + 1),
                                new KeyValuePair<string, object>("resultId", resultId));

                            string type = "";
                            string value = "";
                            switch (resultIndex)
                            {
                                case 0:
                                    type = "int";
                                    value = totalItem.ToString();
                                    break;
                                case 1:
                                    type = "DateTime";
                                    value = DateTime.Parse("1/1/2000").AddDays(totalItem).AddSeconds(totalItem).ToString();
                                    break;
                                case 2:
                                    type = "string";
                                    value = string.Format("string value {0}", totalItem);
                                    break;
                                case 3:
                                    type = "decimal";
                                    value = string.Format("{0}.{0}", totalItem);
                                    break;
                                case 4:
                                    type = "JSON";
                                    value = string.Format("{{ Property1: \"{0}\", Property2: \"Some value ({0})\" }}", totalItem);
                                    break;
                                default:
                                    throw new IndexOutOfRangeException();
                            }

                            totalItem++;

                            store.ExecuteSQL("INSERT INTO Result(Id, EntityId, [Key], Type, Value) VALUES (@id, @entityId, @key, @type, @value)",
                                new KeyValuePair<string, object>("id", resultId),
                                new KeyValuePair<string, object>("entityId", entityId),
                                new KeyValuePair<string, object>("key", string.Format("{{COLUMN{0:00}}}", resultIndex + 1)),
                                new KeyValuePair<string, object>("type", type),
                                new KeyValuePair<string, object>("value", value));
                        }
                    }
                }
            }

            Debug.WriteLine("Inserting records: {0}", sw.Elapsed);

            sw.Reset();
            sw.Start();
            using (var store = new SQLCEStore(defaultDatabase, defaultPassword))
            {
                var table = store.GetDataTable("SELECT Grouping.*, Result.* FROM Grouping JOIN Result ON ResultId=Result.Id ORDER BY Result.EntityId, Grouping.[Order]");
                Debug.WriteLine("Selecting from table: {0}", sw.Elapsed);
            }
        }

        public static void TestDataTableAccess()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MyColumn", typeof(int));
            table.Rows.Add(1);

            int j = 0;
            Stopwatch sw1 = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                j += (int)table.Rows[0]["MyColumn"];
            sw1.Stop();

            j = 0;
            Stopwatch sw2 = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
                j += (int)table.Rows[0]["mycolumn"];
            sw2.Stop();

            Debug.WriteLine("Time: " + sw1.Elapsed);
            Debug.WriteLine("Time: " + sw2.Elapsed);
        }

        [ComImport]
        [Guid("550e8400-e29b-41d4-a716-446655450000")]
        [CoClass(typeof(ConsoleWriter))]
        public interface IMessageWriter
        {
            void WriteHello();
        }

        public class ConsoleWriter : IMessageWriter
        {
            public void WriteHello()
            {
                Console.WriteLine("Hello");
            }
        }

        public static unsafe void Go()
        {
            IMessageWriter messageWriter = new IMessageWriter();
            messageWriter.WriteHello();
            SortedDictionary<string, string> x = new SortedDictionary<string, string>();
        }

        public static unsafe void GoString()
        {
            String first = "Hello";
            Debug.WriteLine(first);
            first.ToUpper();
            Debug.WriteLine(first);
            first.MutateToUpper();
            Debug.WriteLine(first);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union
        {
            [FieldOffset(0)]
            public uint Value;

            [FieldOffset(3)]
            public byte AsByte;
        }

        private static void TraceMe()
        {
            // First step: create the trace source object
            TraceSource ts = new TraceSource("myTraceSource");

            // Writing out some events
            ts.TraceEvent(TraceEventType.Warning, 0, "warning message", "a", "b", "c", 1, 2, 3);
            ts.TraceEvent(TraceEventType.Error, 0, "error message");
            //ts.TraceEvent(TraceEventType.Information, 0, "information message");
            //ts.TraceEvent(TraceEventType.Critical, 0, "critical message");
        }

        private static void Test_Database()
        {
            Database.ExecuteSQL("DELETE FROM Genre");
            Database.ExecuteSQL("INSERT Genre SELECT @GenreId, @Language, @Genre, @UpdatedDate",
                new KeyValuePair<string, object>("GenreId", "1"), // Int type in database gets automatically converted from string
                new KeyValuePair<string, object>("Language", "language"),
                new KeyValuePair<string, object>("Genre", "genre"),
                new KeyValuePair<string, object>("updateddate", DateTime.Now));
        }

        private static void Test_Other(params string[] args)
        {
            Monads.Test();
            int result = new AsyncExamples().MyTaskAsync().Result;

            var tableCreationStatements = SchemaGenerator.GenerateSQL(typeof(programsProgram), 0, "programsProgram", "castTypeMemberName", "crewTypeMemberName").ToList();
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

                var inserts = SchemaGenerator.GenerateSQLInsertStatement(item, loaded.programs.First().TMSId, "castTypeMemberName", "crewTypeMemberName");
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
        }

        private static void Other2(params string[] args)
        {
            var a = TimeSpan.FromSeconds(12345).ToString();
            var b = TimeSpan.FromSeconds(12390).ToString("hh\\:mm\\:ss");

            byte[] bytes1 = new SHA1CryptoServiceProvider().ComputeHash("input1".Select(x => (byte)x).ToArray());
            string hex1 = string.Join(string.Empty, bytes1.Select(x => string.Format("{0:x2}", x)));

            byte[] bytes2 = new SHA1CryptoServiceProvider().ComputeHash("input2".Select(x => (byte)x).ToArray());
            string hex2 = string.Join(string.Empty, bytes2.Select(x => string.Format("{0:x2}", x)));

            LineCounter.CountLines(args.First(), args.Skip(1).ToArray());

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
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
    }

    public static class MyUtility
    {
        public static unsafe void MutateToUpper(this string @this)
        {
            if (@this == null) throw new NullReferenceException("MutateToUpper called on a null string.");

            fixed (char* fixedCharacterArray = @this)
            {
                for (char* character = fixedCharacterArray; *character != 0; character++)
                {
                    *character = char.ToUpper(*character);
                }
            }
        }
    }
}
