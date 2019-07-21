using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KlaxEditor.Utility;

namespace KlaxEditor.Views.KlaxScript
{
    /// <summary>
    /// Interaction logic for AddNodePopup.xaml
    /// </summary>
    public partial class AddNodePopup : UserControl
    {
        public AddNodePopup()
        {
            InitializeComponent();
        }

		private void AddNodePopup_OnLoaded(object sender, RoutedEventArgs e)
		{
			FilterTextBox.Focus();
		}

		private void AddNodePopup_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void AddNodePopup_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			
			//if (e.Key != Key.Down && e.Key != Key.Up)
			//{
			//	return;
			//}

			//ScrollViewer scrollViewer = GetScrollViewer(PossibleNodes);
			//if (scrollViewer != null)
			//{
			//	switch (e.Key)
			//	{
			//		case Key.Down:
			//			scrollViewer.LineDown();
			//			break;
			//		case Key.Up:
			//			scrollViewer.LineUp();
			//			break;
			//	}
			//}
		}

		private ScrollViewer GetScrollViewer(DependencyObject o)
		{
			if (o is ScrollViewer scroller)
			{
				return scroller;
			}

			int childCount = VisualTreeHelper.GetChildrenCount(o);
			for (int i = 0; i < childCount; i++)
			{
				var child = VisualTreeHelper.GetChild(o, i);
				ScrollViewer childScrollViewer = GetScrollViewer(child);
				if (childScrollViewer != null)
				{
					return childScrollViewer;
				}
			}

			return null;
		}

		private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
		{
			//if (!Object.ReferenceEquals(sender, e.OriginalSource))
			//{
			//	return;
			//}
			//TreeViewItem itemSource = (TreeViewItem)sender;
			//itemSource.BringIntoView();
		}
	}
}
