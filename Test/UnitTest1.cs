using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine;
using System.IO;
using System.Diagnostics;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Stream resource = typeof(Class1).Assembly.GetManifestResourceStream("Engine.Resources.prince.exe");

            int pId = Process.GetCurrentProcess().Id;
            string path = Path.Combine(Path.GetTempPath(), pId.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, "prince.exe");
            using (Stream output = File.Create(filePath))
                resource.CopyTo(output);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
