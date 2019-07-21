using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace KlaxEditor.Utility
{
	public static class EditorControlUtility
	{
		public static TChildItem FindVisualChild<TChildItem>(DependencyObject dependencyObject, String name) where TChildItem : DependencyObject
		{
			// Search immediate children first (breadth-first)
			var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

			if (childrenCount == 0 && dependencyObject is Popup)
			{
				var popup = dependencyObject as Popup;
				return popup.Child != null ? FindVisualChild<TChildItem>(popup.Child, name) : null;
			}

			for (var i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(dependencyObject, i);
				var nameOfChild = child.GetValue(FrameworkElement.NameProperty) as String;

				if (child is TChildItem && (name == String.Empty || name == nameOfChild))
					return (TChildItem)child;
				var childOfChild = FindVisualChild<TChildItem>(child, name);
				if (childOfChild != null)
					return childOfChild;
			}
			return null;
		}
	}
}
