using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLReporting
{
    public interface IDataSource
    {
        IEnumerable<string> GetUniqueGroups(params Group[] parentGroups);

        object this[Guid itemID, string key, params Group[] groups] { get; }

        object this[string key, params Group[] groups] { get; }
    }
}
