using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KlaxEditor.UserControls
{
	public class ClickSelectTextBox : TextBox
	{
		public ClickSelectTextBox()
		{
			AddHandler(PreviewMouseLeftButtonDownEvent,  new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
			AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
			AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
		}

		private static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			DependencyObject parent = e.OriginalSource as UIElement;
			while (parent != null && !(parent is TextBox))
				parent = VisualTreeHelper.GetParent(parent);

			if (parent != null)
			{
				var textBox = (TextBox)parent;
				if (!textBox.IsKeyboardFocusWithin)
				{
					textBox.Focus();
					e.Handled = true;
				}
			}
		}

		private static void SelectAllText(object sender, RoutedEventArgs e)
		{
			var textBox = e.OriginalSource as TextBox;
			if (textBox != null)
				textBox.SelectAll();
		}
	}
}
