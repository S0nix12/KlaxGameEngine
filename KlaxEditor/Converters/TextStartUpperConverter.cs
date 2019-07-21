using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KlaxEditor.Converters
{
    class TextStartUpperConverter : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string text)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					return value;
				}

				char[] chars = text.ToCharArray();
				chars[0] = char.ToUpper(chars[0]);
				return new string(chars);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
