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

        public IEnumerable<string> GetUniqueGroups(params Group[] parentGroups)
        {
            var matchingRows = Table.Rows.Where(x => MatchParentGroups(x, parentGroups));
            return matchingRows.Select(x => x[parentGroups.Last().Key]).Distinct().OfType<string>();
        }

        private static bool MatchParentGroups(DataRow row, params Group[] parentGroups)
        {
            foreach (Group group in parentGroups)
            {
                if (group.Value == "*")
                {
                    if (row[group.Key] == DBNull.Value)
                        return false;

                    continue;
                }

                if (!object.Equals(row[group.Key], group.Value))
                    return false;
            }

            return true;
        }

        public object this[Guid itemID, string key, params Group[] groups]
        {
            get
            {
                IEnumerable<DataRow> itemRows = Table.Rows.Where(x => (Guid)x["MainReportItemID"] == itemID);
                return GetObject(itemRows, key, groups);
            }
        }

        public object this[string key, params Group[] groups]
        {
            get
            {
                return GetObject(Table.Rows.ToArray(), key, groups);
            }
        }

        private object GetObject(IEnumerable<DataRow> rows, string key, params Group[] groups)
        {
            IEnumerable<DataRow> filtered = rows.ToList();

            foreach (Group group in groups)
                filtered = filtered.Where(x => object.Equals(x[group.Key], group.Value));

            return filtered.Single()[key];
        }
    }
}
