using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Engine
{
    public static class Utility
    {
        static Utility()
        {
            if (Directory.Exists(CONSTANTS.TEMPDIRECTORY))
                Directory.Delete(CONSTANTS.TEMPDIRECTORY, true);

            Directory.CreateDirectory(CONSTANTS.TEMPDIRECTORY);
        }

        public static void Initialize()
        {
            Stopwatch sw = Stopwatch.StartNew();

            InitializePrince();
            InitializePhantomJS();

            sw.Stop();
        }

        private static void InitializePrince()
        {
            string princeZip = Path.Combine(CONSTANTS.PRINCE_DIRECTORY, "prince.zip");
            using (Stream output = File.Create(princeZip))
                typeof(Class1).Assembly.GetManifestResourceStream("Engine.Resources.Prince.zip").CopyTo(output);

            ZipFile.ExtractToDirectory(princeZip, CONSTANTS.PRINCE_DIRECTORY);
            File.Delete(princeZip);
        }

        private static void InitializePhantomJS()
        {
            using (Stream output = File.Create(Path.Combine(CONSTANTS.PHANTOMJS_DIRECTORY, "phantomjs.exe")))
                typeof(Class1).Assembly.GetManifestResourceStream("Engine.Resources.phantomjs.exe").CopyTo(output);
            using (Stream output = File.Create(Path.Combine(CONSTANTS.PHANTOMJS_DIRECTORY, "phantom.js")))
                typeof(Class1).Assembly.GetManifestResourceStream("Engine.Resources.phantom.js").CopyTo(output);
        }

        public static void Cleanup()
        {
            try
            {
                Directory.Delete(CONSTANTS.TEMPDIRECTORY, true);
            }
            catch (Exception)
            {
                //throw;
            }
        }

        private static IEnumerable<string> ProcessJavaScript(this IEnumerable<string> inputs)
        {
            foreach (var item in inputs)
            {
                XElement element = XElement.Parse(File.ReadAllText(item));
                var needsProcessing = element.DescendantsAndSelf().Any(x => x.Name.LocalName.Trim().Equals("script", StringComparison.InvariantCultureIgnoreCase));

                yield return needsProcessing ? ProcessJavaScript(item) : item;
            }
        }

        private static string ProcessJavaScript(string inputFile)
        {
            string outputFile = inputFile + ".out.html";

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = CONSTANTS.PHANTOMJS_EXECUTABLE;
            process.StartInfo.Arguments = string.Format("{0} -o \"{1}\"", inputFile, outputFile);

            process.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" '{3}' '{4}'", Path.Combine(CONSTANTS.PHANTOMJS_DIRECTORY, "phantom.js"), inputFile, outputFile, 1000, 900);

            process.Start();

            // for debugging
            var command = process.StartInfo.FileName + " " + process.StartInfo.Arguments;

            return outputFile;
        }


        // TODO: there is a wrapper on the internet somewhere for this... I just don't want to get sidetracked
        public static bool RenderXMLAsPDF(string output, params string[] inputs1)
        {
            IEnumerable<string> inputs = inputs1.ProcessJavaScript().ToArray();

            string styleFile = Path.GetTempFileName() + ".css";

            List<string> lines = new List<string>();
            lines.Add(string.Format("@page {{ size: {0} {1} }}", PageSize.US_Letter, "landscape").Replace("_", "-"));

            lines.Add("@page:right { margin: 50pt 10pt 50pt 50pt }");
            lines.Add("@page:left { margin: 50pt 50pt 50pt 10pt }");
            lines.Add("@page { border: solid 2pt orange }");

            lines.Add("@page { @top { content: flow(header) } }");
            lines.Add(".pageheader { flow: static(header) }");

            lines.Add("@page { @bottom { content: flow(footer) } }");
            lines.Add(".pagefooter { flow: static(footer) }");

            lines.Add("span .pagenumber { content: counter(page) }");
            lines.Add("span .pagecount { content: counter(pages) }");

            File.WriteAllLines(styleFile, lines);

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = CONSTANTS.PRINCE_EXECUTABLE;
            process.StartInfo.Arguments = string.Format("{0} -s \"{1}\" -o \"{2}\"", string.Join(" ", inputs.Select(x => string.Format("\"{0}\"", x))), styleFile, output);

            Stopwatch sw = Stopwatch.StartNew();
            process.Start();
            sw.Stop();
            Debug.WriteLine("Prince took " + sw.Elapsed);

            // for debugging
            var command = process.StartInfo.FileName + " " + process.StartInfo.Arguments;

            Process.Start(output);

            return true;
        }
    }
}
