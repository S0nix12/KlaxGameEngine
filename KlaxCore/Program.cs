using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using SharpDX.Direct3D11;

namespace KlaxCore
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game())
            {
                // Force english exception messages because they are f*****g useless in other language
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
                game.Run();
            }

        }
    }
}
