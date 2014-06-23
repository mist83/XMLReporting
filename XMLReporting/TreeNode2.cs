using System;
using System.Collections.Generic;
using System.Linq;

namespace XMLReporting
{
    /// <summary>
    /// 
    /// </summary>
    public class TreeNode2 : TreeNode
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        public TreeNode2(string key)
            : this(null, key) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="group"></param>
        /// <param name="key"></param>
        public TreeNode2(IComparable group, string key)
        {
            Group = group;
            Key = key;
        }

        /// <summary>
        /// Group
        /// </summary>
        public IComparable Group { get; private set; }

        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        public void Fill(IDataSource dataSource)
        {
            // 1) Fill this item


            // 2) Fill all children
            foreach (var item in Children.OfType<TreeNode2>())
            {
                item.Fill(dataSource);
            }
        }

        public override IEnumerable<string> GetUniqueGroups(params Group[] parentGroups)
        {
            throw new NotImplementedException();
        }

        public override object this[Guid itemID, string key, params Group[] groups]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override object this[string key, params Group[] groups]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
