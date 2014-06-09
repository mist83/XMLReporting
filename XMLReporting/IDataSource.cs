using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLReporting
{
    public interface IDataSource
    {
        IEnumerable<string> GetUniqueGroups(params Tuple<string, string>[] parentGroups);

        object this[Guid itemID, string key, params Tuple<string, string>[] groups] { get; }

        object this[string key, params Tuple<string, string>[] groups] { get; }
    }
}
