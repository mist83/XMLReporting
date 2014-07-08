using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class CONSTANTS
    {
        public static string TEMPDIRECTORY { get { return Path.Combine(Path.GetTempPath(), "XMLReporting", string.Format("PID{0}", Process.GetCurrentProcess().Id)); } }

        public static string PRINCE_DIRECTORY
        {
            get
            {
                string directory = Path.Combine(CONSTANTS.TEMPDIRECTORY, "Prince");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
        }

        public static string PRINCE_EXECUTABLE { get { return Path.Combine(PRINCE_DIRECTORY, "bin", "prince.exe"); } }

        public static string PHANTOMJS_DIRECTORY
        {
            get
            {
                string directory = Path.Combine(CONSTANTS.TEMPDIRECTORY, "PhantomJS");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
        }

        public static string PHANTOMJS_EXECUTABLE { get { return Path.Combine(PHANTOMJS_DIRECTORY, "phantomjs.exe"); } }
    }
}
