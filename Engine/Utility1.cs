using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class Utility1
    {
        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            foreach (string directory in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(directory.Replace(sourcePath, destinationPath));
            }

            foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(sourcePath, destinationPath), true);
            }
        }
    }
}
