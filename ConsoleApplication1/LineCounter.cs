using System;
using System.IO;
using System.Linq;

namespace ConsoleApplication1
{
    public class LineCounter
    {
        public static void CountLines(string directory, params string[] fileTypes)
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
    }
}
