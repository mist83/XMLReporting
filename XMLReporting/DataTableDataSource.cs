using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLReporting
{
    public class DataTableDataSource : IDataSource
    {
        public DataTable Table { get; private set; }

        public DataTableDataSource(DataTable table)
        {
            Table = table;
        }

        public IEnumerable<string> GetUniqueGroups(params Tuple<string, string>[] parentGroups)
        {
            var matchingRows = Table.Rows.Where(x => MatchParentGroups(x, parentGroups));
            return matchingRows.Select(x => x[parentGroups.Last().Item1]).Distinct().OfType<string>();
        }

        private static bool MatchParentGroups(DataRow row, params Tuple<string, string>[] parentGroups)
        {
            foreach (Tuple<string, string> item in parentGroups)
            {
                if (item.Item2 == "*")
                {
                    if (row[item.Item1] == DBNull.Value)
                        return false;

                    continue;
                }

                if (!object.Equals(row[item.Item1], item.Item2))
                    return false;
            }

            return true;
        }

        public object this[Guid itemID, string key, params Tuple<string, string>[] groups]
        {
            get
            {
                IEnumerable<DataRow> itemRows = Table.Rows.Where(x => (Guid)x["MainReportItemID"] == itemID);
                return GetObject(itemRows, key, groups);
            }
        }

        public object this[string key, params Tuple<string, string>[] groups]
        {
            get
            {
                return GetObject(Table.Rows.ToArray(), key, groups);
            }
        }

        private object GetObject(IEnumerable<DataRow> rows, string key, params Tuple<string, string>[] groups)
        {
            IEnumerable<DataRow> filtered = rows.ToList();
            foreach (Tuple<string, string> tuple in groups)
                filtered = filtered.Where(x => object.Equals(x[tuple.Item1], tuple.Item2));

            return filtered.Single()[key];
        }
    }
}
