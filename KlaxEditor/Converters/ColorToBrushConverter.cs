using KlaxEditor.Utility;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace KlaxEditor.Converters
{
	[ValueConversion(typeof(Color4), typeof(System.Windows.Media.SolidColorBrush))]
	public sealed class ColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color4 color)
			{
				return new System.Windows.Media.SolidColorBrush(EditorConversionUtility.ConvertEngineColorToSystem(color));
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is System.Windows.Media.SolidColorBrush brush)
			{
				return EditorConversionUtility.ConvertSystemColorToEngine(brush.Color);
			}

			return null;
		}
	}
}
