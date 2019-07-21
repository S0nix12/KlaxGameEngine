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
	class VisibilityCombineMultiConverter : IMultiValueConverter
	{
		public bool ReturnMostVisible { get; set; }

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (ReturnMostVisible)
			{
				Visibility mostVisible = Visibility.Collapsed;
				foreach (var o in values)
				{
					Visibility visibility = (Visibility) o;
					if (visibility == Visibility.Visible)
					{
						return visibility;
					}
					else
					{
						mostVisible = mostVisible < visibility ? mostVisible : visibility;
					}
				}

				return mostVisible;
			}
			else
			{
				Visibility leastVisible = Visibility.Visible;
				foreach (var o in values)
				{
					Visibility visibility = (Visibility)o;
					if (visibility == Visibility.Collapsed)
					{
						return visibility;
					}
					else
					{
						leastVisible = leastVisible > visibility ? leastVisible : visibility;
					}
				}

				return leastVisible;
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
