using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMLReporting.Test
{
    [TestClass]
    public class TreeNodeTestClass
    {
        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void Node_Already_Has_Parent()
        {
            TreeNode a = new TreeNode("a");
            TreeNode b = new TreeNode("b");
            TreeNode c = new TreeNode("c");
            a.Add(b);
            b.Add(c);
            a.Add(c);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void UngroupedChildAdded()
        {
            TreeNode a = TreeNode.CreateRoot();
            TreeNode b = TreeNode.CreateRoot();
            a.Add(b); // b has not been grouped, so it cannot be added
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void Duplicate_Group()
        {
            TreeNode a = new TreeNode("group a");
            TreeNode b = new TreeNode("group b");
            TreeNode c = new TreeNode("group a");
            a.Add(b);
            b.Add(c);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void Cycle()
        {
            TreeNode a = new TreeNode("a");
            TreeNode b = new TreeNode("b");
            TreeNode c = new TreeNode("c");
            a.Add(b);
            b.Add(c);
            c.Add(a);
        }

        [TestMethod]
        public void GetLeafNodes()
        {
            var root = TreeNode.CreateRoot();
            root.Add(new TreeNode("group 1")).Add(new TreeNode("group 2"));

            Assert.AreEqual(root.Children.First().Children.First(), root.GetLeafNodes().Single());
            Assert.AreEqual(root.Children.First().Children.First(), root.Children.First().GetLeafNodes().Single());
            Assert.AreEqual(root.Children.First().Children.First(), root.Children.First().Children.First().GetLeafNodes().Single());
        }

        [TestMethod]
        public void MyTestMethod()
        {
            TreeNode node = new TreeNode("a") { Results = new[] { new NodeResult { Key = "b", Value = "c" } } }.Add(new TreeNode("d") { Results = new[] { new NodeResult { Key = "e", Value = "h" } } });
            var serialized = node.Serialize();
            TreeNode deserialized1 = TreeNode.Deserialize(serialized);

            TreeNode deserialized2 = TreeNode.Deserialize(@"{""Children"":[{""Children"":[],""Group"":""d"",""Key"":""e"",""Value"":""f""}],""Group"":""a"",""Key"":""b"",""Value"":""c""}");
        }

        [TestMethod]
        public void Consolidation_Decimal()
        {
            TreeNode root = TreeNode.CreateRoot();
            root.Results = new[] { new NodeResult { Key = "a" } };
            root.Add(new TreeNode("group1", new NodeResult { Key = "a", Value = 1M }));
            root.Add(new TreeNode("group2", new NodeResult { Key = "a", Value = 2M }));

            Assert.AreEqual(3M, root.ConsolidateLeaves().Results.Single().Value);
        }

        [TestMethod]
        public void Consolidation_String_One_Item()
        {
            TreeNode root = TreeNode.CreateRoot();
            root.Results = new[] { new NodeResult { Key = "a" } };
            root.Add(new TreeNode("group1", new NodeResult { Key = "a", Value = "Value 1" }));

            Assert.AreEqual("Value 1", root.ConsolidateLeaves().Results.Single().Value);
        }

        [TestMethod]
        public void Consolidation_Decimal_Many_Items()
        {
            TreeNode root = TreeNode.CreateRoot();
            root.Results = new[] { new NodeResult { Key = "a" } };
            root.Add(new TreeNode("group1", new NodeResult { Key = "a", Value = "Value 1" }));
            root.Add(new TreeNode("group2", new NodeResult { Key = "a", Value = "Value 2" }));

            Assert.AreEqual("(2 items)", root.ConsolidateLeaves().Results.Single().Value);
        }
    }
}
