using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Engine
{
    public class ReportingEngine : IDisposable
    {
        // All of the temporary directories used by this engine
        private static HashSet<string> tempDirectories = new HashSet<string>();

        // All of the streams embedded into this dll (there's no sense in 
        private static Stream _PrinceStream = null;
        private static Stream _PhantomJSExecutableStream = null;
        private static Stream _PhantomJSFileStream = null;
        private static string _ExtractedPrinceDirectory = null;

        private string tempDirectory = null;
        private static object lockObject = new object();

        public static Stream PrinceStream
        {
            get
            {
                if (_PrinceStream == null)
                {
                    _PrinceStream = typeof(ReportingEngine).Assembly.GetManifestResourceStream("Engine.Resources.Prince.zip.resource");
                }

                _PrinceStream.Position = 0;
                return _PrinceStream;
            }
        }

        public Stream PhantomJSExecutableStream
        {
            get
            {
                if (_PhantomJSExecutableStream == null)
                {
                    _PhantomJSExecutableStream = typeof(ReportingEngine).Assembly.GetManifestResourceStream("Engine.Resources.phantomjs.exe.resource");
                }

                _PhantomJSExecutableStream.Position = 0;
                return _PhantomJSExecutableStream;
            }
        }

        public Stream PhantomJSFileStream
        {
            get
            {
                if (_PhantomJSFileStream == null)
                {
                    _PhantomJSFileStream = typeof(ReportingEngine).Assembly.GetManifestResourceStream("Engine.Resources.phantom.js");
                }

                _PhantomJSFileStream.Position = 0;
                return _PhantomJSFileStream;
            }
        }

        public static string ExtractedPrinceDirectory
        {
            get
            {
                if (_ExtractedPrinceDirectory == null || !Directory.Exists(_ExtractedPrinceDirectory))
                {
                    _ExtractedPrinceDirectory = Path.Combine(Path.GetTempPath(), "XMLReporting", "Resources", "Prince");
                    Directory.CreateDirectory(_ExtractedPrinceDirectory);

                    string princeZip = Path.Combine(Path.GetTempPath(), "XMLReporting", "Resources", "Prince", "prince.zip");
                    using (Stream output = File.Create(princeZip))
                    {
                        PrinceStream.CopyTo(output);
                    }

                    ZipFile.ExtractToDirectory(princeZip, _ExtractedPrinceDirectory);
                    File.Delete(princeZip);
                }

                return _ExtractedPrinceDirectory;
            }
        }

        public string PRINCE_DIRECTORY
        {
            get
            {
                string directory = Path.Combine(tempDirectory, "Prince");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
        }

        public string PRINCE_EXECUTABLE { get { return Path.Combine(PRINCE_DIRECTORY, "bin", "prince.exe"); } }

        public string PHANTOMJS_DIRECTORY
        {
            get
            {
                string directory = Path.Combine(tempDirectory, "PhantomJS");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
        }

        public string PHANTOMJS_EXECUTABLE { get { return Path.Combine(PHANTOMJS_DIRECTORY, "phantomjs.exe"); } }

        static ReportingEngine()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            string rootDirectory = Path.Combine(Path.GetTempPath(), "XMLReporting");
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, true);
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Cleanup();
        }

        private static TimeSpan twConstructorTime = new TimeSpan();
        private static Stopwatch swCleanup = new Stopwatch();

        public ReportingEngine()
        {
            lock (lockObject)
            {
                Stopwatch swConstructor = Stopwatch.StartNew();

                tempDirectory = Path.Combine(Path.GetTempPath(), "XMLReporting", string.Format("ThreadId_{0}", Thread.CurrentThread.ManagedThreadId));

                // Only bother copying the files if we're working in a separate thread
                if (!tempDirectories.Contains(tempDirectory))
                {
                    tempDirectories.Add(tempDirectory);

                    Utility1.CopyDirectory(ExtractedPrinceDirectory, PRINCE_DIRECTORY);

                    using (Stream output = File.Create(Path.Combine(PHANTOMJS_DIRECTORY, "phantomjs.exe")))
                    {
                        PhantomJSExecutableStream.CopyTo(output);
                    }

                    using (Stream output = File.Create(Path.Combine(PHANTOMJS_DIRECTORY, "phantom.js")))
                    {
                        PhantomJSFileStream.CopyTo(output);
                    }
                }

                swConstructor.Stop();
                twConstructorTime = twConstructorTime.Add(swConstructor.Elapsed);
            }
        }

        private static void Cleanup(params string[] directories)
        {
            return;

            swCleanup.Start();

            foreach (var directory in directories)
            {
                try
                {
                    if (!Directory.Exists(directory))
                    {
                        continue;
                    }

                    Directory.Delete(directory, true);
                }
                catch
                {
                    //throw;
                }
            }

            swCleanup.Stop();
        }

        private IEnumerable<string> ProcessJavaScript(IEnumerable<string> inputs)
        {
            foreach (var item in inputs)
            {
                // Default assumption is that the file has JavaScript that needs to be processed
                bool needsProcessing = true;

                try
                {
                    XElement element = XElement.Parse(File.ReadAllText(item));

                    // Only if the file is successfully opened and has no "<script>" tags will we assume that it doesn't need any processing
                    needsProcessing = element.DescendantsAndSelf().Any(x => x.Name.LocalName.Trim().Equals("script", StringComparison.InvariantCultureIgnoreCase));
                }
                catch { }

                yield return needsProcessing ? ProcessJavaScript(item) : item;
            }
        }

        private string ProcessJavaScript(string inputFile)
        {
            string outputFile = Path.Combine(tempDirectory, "ProcessedJavaScript", string.Format("{0}.{1}{2}", Path.GetFileNameWithoutExtension(inputFile), DateTime.Now.ToString("hh_MM_ss"), Path.GetExtension(inputFile)));
            if (!Directory.Exists(new FileInfo(outputFile).DirectoryName))
            {
                Directory.CreateDirectory(new FileInfo(outputFile).DirectoryName);
            }

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = PHANTOMJS_EXECUTABLE;
            process.StartInfo.Arguments = string.Format("{0} -o \"{1}\"", inputFile, outputFile);

            process.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" '{3}' '{4}'", Path.Combine(PHANTOMJS_DIRECTORY, "phantom.js"), inputFile, outputFile, 1000, 900);

            process.Start();
            process.WaitForExit();

            // for debugging
            Debug.WriteLine(process.StartInfo.FileName + " " + process.StartInfo.Arguments);

            return outputFile;
        }

        // TODO: there is a wrapper on the internet somewhere for this... I just don't want to get sidetracked
        public bool RenderXMLAsPDF(string output, params string[] inputs)
        {
            if (!Directory.Exists(Path.GetDirectoryName(output)))
                Directory.CreateDirectory(Path.GetDirectoryName(output));

            IEnumerable<string> processedFiles = ProcessJavaScript(inputs).ToArray();

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
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = PRINCE_EXECUTABLE;
            process.StartInfo.Arguments = string.Format("{0} -s \"{1}\" -o \"{2}\"", string.Join(" ", processedFiles.Select(x => string.Format("\"{0}\"", x))), styleFile, output);

            Stopwatch sw = Stopwatch.StartNew();
            process.Start();
            process.WaitForExit();

            sw.Stop();
            Debug.WriteLine("Prince took " + sw.Elapsed);

            // for debugging
            Debug.WriteLine(process.StartInfo.FileName + " " + process.StartInfo.Arguments);

            long length = File.OpenRead(output).Length;
            if (length != 41929)
            {

            }

            //Process.Start(output);

            return true;
        }

        public void Dispose()
        {
            Cleanup(tempDirectory);
        }
    }
}
