using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Database
    {
        public static string ConnectionString { get { return new SqlConnectionStringBuilder { DataSource = "dlabtwsql121", InitialCatalog = "TMS_Test", IntegratedSecurity = true }.ToString(); } }

        public static void ExecuteSQL(string command, params KeyValuePair<string, object>[] parameters)
        {
            var matches = Regex.Matches(command, @"@\w+");

            if (matches.Count != parameters.Length)
                throw new Exception(string.Format("Statement expects {0} parameters, but {1} parameters were supplied", matches.Count, parameters.Length));

            foreach (var item in matches.OfType<Match>())
            {
                int parameterMatchCount = parameters.Select(x => x.Key.TrimStart('@')).Count(x => x.Equals(item.Value.TrimStart('@'), StringComparison.InvariantCultureIgnoreCase));
                if (parameterMatchCount != 1)
                    throw new Exception(string.Format("No parameter/multiple parameters found for '{0}' in parameters collection ({1})", item.Value, string.Join(", ", parameters.Select(x => x.Key))));
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(command, connection))
                {
                    foreach (var item in parameters)
                    {
                        sqlCommand.Parameters.Add(new SqlParameter(item.Key, item.Value));
                    }

                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static DataTable GetDataTable(string command)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(command, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);

                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    foreach (DataRow row in table.Rows)
                    {
                        foreach (DataColumn dc in table.Columns)
                        {
                            Console.Write(row[dc.ColumnName] + ", ");
                        }

                        Console.WriteLine();
                    }

                    return table;
                }
            }
        }
    }
}
