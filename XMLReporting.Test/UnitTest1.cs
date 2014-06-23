using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMLReporting.Test
{
    [TestClass]
    public class UnitTest1
    {
        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void Node_Already_Has_Parent()
        {
            TreeNode a = new TreeNode2("a");
            TreeNode b = new TreeNode2("b");
            TreeNode c = new TreeNode2("c");
            a.Add(b);
            b.Add(c);
            a.Add(c);
        }

        [ExpectedException(typeof(Exception))]
        [TestMethod]
        public void Cycle()
        {
            TreeNode a = new TreeNode2("a");
            TreeNode b = new TreeNode2("b");
            TreeNode c = new TreeNode2("c");
            a.Add(b);
            b.Add(c);
            c.Add(a);
        }
    }
}
