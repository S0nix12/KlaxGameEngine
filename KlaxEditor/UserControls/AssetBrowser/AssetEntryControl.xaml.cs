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

namespace KlaxEditor.UserControls.AssetBrowser
{
	/// <summary>
	/// Interaction logic for AssetEntryControl.xaml
	/// </summary>
	public partial class AssetEntryControl : UserControl
	{
		public AssetEntryControl()
		{
			InitializeComponent();
		}

		private void AssetTextLabel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				AssetTextBox.Visibility = Visibility.Visible;
				AssetTextLabel.Visibility = Visibility.Collapsed;
				m_editStartText = AssetTextLabel.Text;
				AssetTextBox.Focus();
				IsInEditMode = true;
				e.Handled = true;
			}
		}

		private void AssetTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			AssetTextBox.Visibility = Visibility.Collapsed;
			AssetTextLabel.Visibility = Visibility.Visible;
			IsInEditMode = false;
		}

		public bool IsInEditMode { get; private set; }

		private void AssetTextBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AssetTextBox.Visibility = Visibility.Collapsed;
			}
			else if(e.Key == Key.Escape)
			{
				AssetTextBox.Text = m_editStartText;
				AssetTextBox.Visibility = Visibility.Collapsed;
			}
		}

		private string m_editStartText;
	}
}
