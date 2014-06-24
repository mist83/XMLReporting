using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace XMLReporting
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TreeNode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="group"></param>
        /// <param name="results"></param>
        public TreeNode(IComparable group, params NodeResult[] results)
            : this()
        {
            Group = group;
            Results = results;
        }

        /// <summary>
        /// Group
        /// </summary>
        [DataMember]
        public IComparable Group { get; private set; }

        /// <summary>
        /// Key
        /// </summary>
        [DataMember]
        public NodeResult[] Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        public void Fill(IDataSource dataSource)
        {
            // 1) Fill this item


            // 2) Fill all children
            foreach (var item in Children.OfType<TreeNode>())
            {
                item.Fill(dataSource);
            }
        }

        public IEnumerable<string> GetUniqueGroups(params Group[] parentGroups)
        {
            throw new NotImplementedException();
        }

        public object this[Guid itemID, string key, params Group[] groups]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object this[string key, params Group[] groups]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
