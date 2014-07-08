using Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Utility.Initialize();
            Utility.RenderXMLAsPDF(@"C:\Users\Michael\Desktop\PCL.pdf", @"Z:\Projects\XMLReporting\XML Page 1.html", @"Z:\Projects\XMLReporting\XML Page 2.html");
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Utility.Cleanup();
        }
    }
}
