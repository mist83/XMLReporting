using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using XMLReporting;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //DrawRedditAlien(width: 60);
            var a = TimeSpan.FromSeconds(12345).ToString();
            var b = TimeSpan.FromSeconds(12390).ToString("hh\\:mm\\:ss");

            byte[] bytes1 = new SHA1CryptoServiceProvider().ComputeHash("input1".Select(x => (byte)x).ToArray());
            string hex1 = string.Join(string.Empty, bytes1.Select(x => string.Format("{0:x2}", x)));

            byte[] bytes2 = new SHA1CryptoServiceProvider().ComputeHash("input2".Select(x => (byte)x).ToArray());
            string hex2 = string.Join(string.Empty, bytes2.Select(x => string.Format("{0:x2}", x)));

            #region other

            if ("a"[0] == 'a')
            {
                Stopwatch sw = Stopwatch.StartNew();

                string xml = File.ReadAllText(@"C:\Users\mullman\documents\visual studio 2013\Projects\ConsoleApplication1\XMLReporting\SampleFiles\All.xhtml");
                for (int i = 0; i < 1; i++)
                {
                    string textReplacementRegex = @"\{Column_[^\}]+\}";
                    var textReplacements = TextReplacementUtility.GetTextReplacements(xml, textReplacementRegex);
                    var dataSource = textReplacements.BuildMockDataSource();
                    var expandedXML = TextReplacementUtility.Apply(xml, dataSource, textReplacementRegex);

                    var expandedTextReplacements = TextReplacementUtility.GetTextReplacements(expandedXML, textReplacementRegex);
                    var evaluatedTextReplacements = expandedTextReplacements.Select(x => x.Apply(dataSource)).ToArray();
                }

                sw.Stop();

                Console.WriteLine();
                TestAbort();
            }

            #endregion

            CountLines(args.First(), args.Skip(1).ToArray());

            Console.Write("Press any key to continue . . . ");
            Console.ReadKey();
        }

        private static void TestAbort()
        {
            Thread t = new Thread(new ThreadStart(abortme));
            t.Start();
            Thread.Sleep(200);
            t.Abort();
            Thread.Sleep(1000);

            Console.Write("Press any key to continue . . .");
            Console.ReadKey();
            return;
        }

        private static void abortme()
        {
            try
            {
                Thread.Sleep(1000);
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Thread was aborted.");
                try
                {
                    return;
                }
                catch
                {
                    Console.WriteLine("Can't even call 'return' on aborted thread");
                }
            }
        }

        #region Draw the Reddit Alien

        static void DrawRedditAlien(string textToUse = "REDDIT*", int? width = null)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            var bitmap = (Bitmap)GetImageFromUrl(@"http://upload.wikimedia.org/wikipedia/fr/f/fc/Reddit-alien.png");

            var rectangle = GetAlphaBoundingRect(bitmap);
            var bm = new Bitmap(rectangle.Width, rectangle.Height);

            // Copy the bits from the old image to the new one
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    var color = bitmap.GetPixel(x + rectangle.Left, y + rectangle.Top);
                    bm.SetPixel(x, y, color);
                }
            }

            bitmap = bm;

            if (!width.HasValue)
                width = Console.BufferWidth;

            double resizeFactor = (double)width / (double)bitmap.Width;
            Size newSize = new Size((int)((double)bitmap.Width * resizeFactor) - 1, (int)((double)bitmap.Width * resizeFactor * .8) - 1);
            bitmap = new Bitmap(bitmap, newSize);

            int characterIndex = 0;

            var colors = new HashSet<Color>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    ConsoleColor? overrideColor = null;

                    // Eyes
                    if (color.ToString() == "Color [A=255, R=255, G=86, B=0]")
                        overrideColor = ConsoleColor.Red;

                    // Outline
                    if (color.A != 0 && color.R == 0 && color.G == 0 && color.B == 0)
                        overrideColor = ConsoleColor.DarkGray;

                    // Track colors
                    if (color.A != 0 && (color.R != 0 || color.G != 0 || color.B != 0))
                        colors.Add(color);

                    // Show the color as white or one of the predefined colors
                    if (overrideColor.HasValue || (color.R != 0 && color.G != 0 && color.B != 0))
                    {
                        ConsoleColor defaultColor = Console.ForegroundColor;

                        if (overrideColor.HasValue)
                            Console.ForegroundColor = overrideColor.Value;

                        Console.Write(textToUse[characterIndex % textToUse.Length]);
                        characterIndex++;
                        Console.ForegroundColor = defaultColor;
                    }
                    else
                        Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private static Rectangle GetAlphaBoundingRect(Bitmap bitmap)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int width = int.MinValue;
            int height = int.MinValue;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (x < left)
                            left = x;

                        if (y < top)
                            top = y;

                        if (x > width)
                            width = x;

                        if (y > height)
                            height = y;
                    }
                }
            }

            return new Rectangle(left, top, width - left, height - top);
        }

        #endregion

        #region count lines

        static void CountLines(string directory, params string[] fileTypes)
        {
            var initialColor = Console.ForegroundColor;

            var total = 0;
            var totalFiles = 0;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Code line count for: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(directory);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");
            foreach (var item in fileTypes)
            {
                var files = Directory.GetFiles(directory, "*." + item, SearchOption.AllDirectories);
                totalFiles += files.Length;

                var codeLines = files.Sum(x => File.ReadAllLines(x).Length);
                total += codeLines;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("|");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("*.{0}", item.ToString().PadRight(10));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(string.Format("{0,4} file(s)", files.Length).PadRight(12));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("{0,16:n0} line(s)", codeLines);

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("|");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("|");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("{0}", "total".PadRight(12));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(string.Format("{0,4} file(s)", totalFiles).PadRight(12));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{0,16:n0} line(s)", total);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("|");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--------------------------------------------------");

            Console.ForegroundColor = initialColor;
        }

        #endregion
    }
}
