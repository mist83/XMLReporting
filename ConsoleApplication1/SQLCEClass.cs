using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    public class SQLCEStore : IDisposable
    {
        private string _fileName = null;
        private string _password = null;

        // TODO: this should probably be kept open for the life of the program (no sense in opening/closing, as it's a local file),
        // but it's not currently because we pass the db/password into the constructor, and those values may differ (and we would have to create a dictionary)
        private SqlCeConnection _connection = null;

        private SqlCeConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqlCeConnection(string.Format("DataSource=\"{0}\"; Password=\"{1}\"", _fileName, _password));
                    _connection.Open();
                }

                return _connection;
            }
        }

        public SQLCEStore(string fileName, string password, bool createDatabase = false)
        {
            _fileName = fileName;
            _password = password;

            if (createDatabase)
            {
                if (File.Exists(_fileName))
                {
                    File.Delete(_fileName);
                }

                new SqlCeEngine(string.Format("DataSource=\"{0}\"; Password=\"{1}\"", _fileName, _password)).CreateDatabase();
            }
        }

        public void ExecuteSQL(string command, params KeyValuePair<string, object>[] parameters)
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

            using (SqlCeCommand sqlCommand = new SqlCeCommand(command, Connection))
            {
                foreach (var item in parameters)
                {
                    sqlCommand.Parameters.Add(new SqlCeParameter(item.Key, item.Value));
                }

                sqlCommand.ExecuteNonQuery();
            }
        }

        public DataTable GetDataTable(string query)
        {
            DataTable dataTable = new DataTable();

            SqlCeDataAdapter adapter = new SqlCeDataAdapter(query, Connection);
            adapter.Fill(dataTable);

            return dataTable;
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
