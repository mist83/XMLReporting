using System;
using System.Collections.Generic;

namespace XMLReporting
{
    /// <summary>
    /// Tree Node
    /// </summary>
    public abstract class TreeNode : IDataSource
    {
        private List<TreeNode> _Children = new List<TreeNode>();

        /// <summary>
        /// Constructor
        /// </summary>
        public TreeNode() { }

        /// <summary>
        /// Parent
        /// </summary>
        public TreeNode Parent { get; private set; }

        /// <summary>
        /// Children
        /// </summary>
        public IReadOnlyCollection<TreeNode> Children { get { return _Children.AsReadOnly(); } }

        /// <summary>
        /// Add a child node
        /// </summary>
        /// <param name="child"></param>
        public TreeNode Add(TreeNode child)
        {
            var tree = CurrentTree;

            if (child.Parent != null)
                throw new Exception("Item already has parent.");
            if (tree.Contains(child))
                throw new Exception("Parent tree already contains this item.");

            child.Parent = this;
            _Children.Add(child);

            return this; // Allow method chaining
        }

        private HashSet<TreeNode> CurrentTree
        {
            get
            {
                HashSet<TreeNode> tree = new HashSet<TreeNode>();

                TreeNode current = this;
                while (current.Parent != null)
                    current = current.Parent;

                tree.Add(current);

                var allChildren = GetAllChildren(current);
                foreach (var item in allChildren)
                {
                    tree.Add(item);
                }

                return tree;
            }
        }

        private List<TreeNode> GetAllChildren(TreeNode current)
        {
            List<TreeNode> allChildren = new List<TreeNode>();

            allChildren.AddRange(current.Children);
            foreach (var item in current.Children)
            {
                var children = item.GetAllChildren(item);
                allChildren.AddRange(children);
            }

            return allChildren;
        }

        public abstract IEnumerable<string> GetUniqueGroups(params Group[] parentGroups);

        public abstract object this[Guid itemID, string key, params Group[] groups] { get; }

        public abstract object this[string key, params Group[] groups] { get; }
    }
}
