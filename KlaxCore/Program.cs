using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxRenderer;
using SharpDX.Direct3D11;

namespace KlaxCore
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CRenderer renderer = new CRenderer())
            {
                // Force english exception messages because they are f*****g useless in other language
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
                renderer.Init("Game", 1280, 720);
                renderer.Run();
            }

        }
    }
}
