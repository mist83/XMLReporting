using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace XMLReporting
{
    /// <summary>
    /// Tree Node
    /// </summary>
    [DataContract]
    public partial class TreeNode : IDataSource
    {
        public static TreeNode CreateRoot()
        {
            return new TreeNode { Results = new NodeResult[] { } };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private TreeNode()
        {
            Children = new List<TreeNode>();
        }

        /// <summary>
        /// Parent
        /// </summary>
        public TreeNode Parent { get; private set; }

        /// <summary>
        /// Children
        /// </summary>
        [DataMember]
        public List<TreeNode> Children { get; private set; }

        /// <summary>
        /// Add a child node
        /// </summary>
        /// <param name="child"></param>
        public TreeNode Add(TreeNode child)
        {
            if (child.Group == null)
                throw new Exception("Node has not been grouped, so it cannot be a child node.");

            if (child.Parent != null)
                throw new Exception("Node already has parent.");

            var tree = CurrentTree;
            if (tree.Contains(child))
                throw new Exception("Parent tree already contains this node.");

            if (tree.Any(x => object.Equals(x.Group, child.Group)))
                throw new Exception("Attempting to add node with duplicate group already in tree");

            child.Parent = this;
            Children.Add(child);

            return child; // Allow method chaining
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

        public IEnumerable<TreeNode> GetLeafNodes()
        {
            List<TreeNode> leafNodes = new List<TreeNode>();

            if (!Children.Any())
                leafNodes.Add(this);

            foreach (var item in Children)
            {
                var childLeafNodes = item.GetLeafNodes();
                leafNodes.AddRange(childLeafNodes);
            }

            return leafNodes;
        }

        public TreeNode ConsolidateLeaves(Func<NodeResult[], object> summationFunction = null)
        {
            if (summationFunction == null)
                summationFunction = DefaultSummations;

            foreach (var item in Children)
            {
                item.ConsolidateLeaves(summationFunction);
            }

            var keys = Children.SelectMany(x => x.Results).Select(x => x.Key).Distinct().ToArray();
            foreach (string key in keys)
            {
                // Don't bother summing values that don't exist at this level
                if (!Results.Any(x => x.Key == key))
                    continue;

                var results = new List<NodeResult>();
                foreach (var item in Children)
                {
                    var result = item.Results.Single(x => x.Key == key);
                    results.Add(result);
                }

                // Set the summed value
                Results.Single(x => x.Key == key).Value = summationFunction.Invoke(results.ToArray());
            }

            return this;
        }

        #region Consolidation functions

        public static object DefaultSummations(params NodeResult[] results)
        {
            var nonNullResults = results.Where(x => x.Value != null);
            var types = nonNullResults.Select(x => x.Value.GetType()).Distinct();

            if (types.Single() == typeof(decimal))
                return DecimalSummationFunction(nonNullResults.ToArray());
            if (types.Single() == typeof(string))
                return StringSummationFunction(nonNullResults.ToArray());

            throw new NotImplementedException(types.Single().Name);
        }

        public static object DecimalSummationFunction(params NodeResult[] results)
        {
            return results.Select(x => x.Value).OfType<decimal>().Sum();
        }

        public static object StringSummationFunction(params NodeResult[] results)
        {
            var strings = results.Select(x => x.Value).OfType<string>().ToArray();
            if (strings.Length == 1)
                return string.Join(", ", strings);

            return string.Format("({0} items)", strings.Length);
        }

        #endregion

        #region Serialization/Deserialization

        public string Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(this.GetType());
                serializer.WriteObject(stream, this);

                var value = stream.ContentToString();
                return value;
            }
        }

        /// <summary>
        /// Deserialize a TreeNode from a JSON string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static TreeNode Deserialize(string json)
        {
            TreeNode deserialized = new DataContractJsonSerializer(typeof(TreeNode)).ReadObject<TreeNode>(json.ToStream());
            RelinkParents(deserialized);
            return deserialized;
        }

        private static void RelinkParents(TreeNode root)
        {
            foreach (TreeNode child in root.Children)
            {
                child.Parent = root;
            }
        }

        #endregion
    }
}
