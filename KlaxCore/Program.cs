using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KlaxConfig;
using KlaxCore.Core;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxShared;
using KlaxShared.Attributes;
using SharpDX.Direct3D11;
using SharpDX.Windows;

namespace KlaxCore
{
    class Program
    {
		[STAThread]
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");

	        using (KlaxEngineForm engineForm = new KlaxEngineForm())
	        {
		        engineForm.CreateEngineAndShow();
		        Application.Run(engineForm);
	        }
		}
	}
}
