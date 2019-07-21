using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxEditor.Utility
{
    public static class EditorConversionUtility
	{
		public static Color4 ConvertSystemColorToEngine(System.Windows.Media.Color systemColor)
		{
			Color4 engineColor = new Color4
			{
				Red = (float)systemColor.R / 255,
				Green = (float)systemColor.G / 255,
				Blue = (float)systemColor.B / 255,
				Alpha = (float)systemColor.A / 255
			};
			return engineColor;
		}

		public static System.Windows.Media.Color ConvertEngineColorToSystem(Color4 engineColor)
		{
			var windowsColor = new System.Windows.Media.Color();
			int rgba = engineColor.ToRgba();
			windowsColor.A = (byte)((rgba >> 24) & 0xFF);
			windowsColor.B = (byte)((rgba >> 16) & 0xFF);
			windowsColor.G = (byte)((rgba >> 8) & 0xFF);
			windowsColor.R = (byte)(rgba & 0xFF);

			return windowsColor;
		}
	}
}
