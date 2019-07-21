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
	[ValueConversion(typeof(int), typeof(Visibility))]
    sealed class LodToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int lod = (int) value;
			int maxLod = parameter != null ? int.Parse(parameter.ToString()) : 0;

			return lod > maxLod ? Visibility.Hidden : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
