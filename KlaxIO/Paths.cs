using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KlaxIO
{
    public static class Paths
    {
        const int PARENT_DIRECTORIES_ABOVE_ASSEMBLY = 0;

        public static string RootDirectory { get; private set; }
        public static string UserDirectory { get; set; }

        static Paths()
        {
            FetchRootDirectory();
        }

        private static void FetchRootDirectory()
        {
            //todo Valentin: Find a better way to determine root folder
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            for (int i = 0; i < PARENT_DIRECTORIES_ABOVE_ASSEMBLY; ++i)
            {
                info = info.Parent;
            }

            RootDirectory = info.FullName;
        }
    }
}
