using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XMLReporting.Test
{
    [TestClass]
    public class TextReplacementTestClass
    {
        [TestMethod]
        public void MyTestMethod()
        {
            string xml = File.ReadAllText(@"C:\Users\mullman\documents\visual studio 2013\Projects\ConsoleApplication1\XMLReporting\SampleFiles\All.xhtml");

            string textReplacementRegex = @"\{Column_[^\}]+\}";
            var textReplacements = TextReplacementUtility.GetTextReplacements(xml, textReplacementRegex);

            var treeNode = TreeNode.CreateRoot();
            var applied = TextReplacementUtility.Apply(xml, treeNode, textReplacementRegex);
        }
    }
}
