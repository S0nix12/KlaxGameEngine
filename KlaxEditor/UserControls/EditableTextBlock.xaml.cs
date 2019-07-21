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

namespace KlaxEditor.UserControls
{
	/// <summary>
	/// Interaction logic for EditableTextBlock.xaml
	/// </summary>
	public partial class EditableTextBlock : UserControl
	{
		public EditableTextBlock()
		{
			InitializeComponent();
		}

		private void LabelText_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				IsInEditMode = true;
				e.Handled = true;
				return;
			}
			
			if (AlwaysHandleClick)
			{
				e.Handled = true;
			}
		}

		private void AssetTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			IsInEditMode = false;
		}

		private void AssetTextBox_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AssetTextBox.Visibility = Visibility.Collapsed;
			}
			else if (e.Key == Key.Escape)
			{
				AssetTextBox.Text = m_editStartText;
				AssetTextBox.Visibility = Visibility.Collapsed;
			}
		}

		#region Dependency Properties
		public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
			"Label", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(default(string)));

		public string Label
		{
			get { return (string)GetValue(LabelProperty); }
			set { SetValue(LabelProperty, value); }
		}

		public static readonly DependencyProperty EditTextProperty = DependencyProperty.Register(
			"EditText", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(default(string)));

		public string EditText
		{
			get { return (string)GetValue(EditTextProperty); }
			set { SetValue(EditTextProperty, value); }
		}

		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register(
			"IsInEditMode", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(default(bool)));

		public bool IsInEditMode
		{
			get { return (bool)GetValue(IsInEditModeProperty); }
			set
			{
				SetValue(IsInEditModeProperty, value);
				if (value)
				{
					AssetTextBox.Visibility = Visibility.Visible;
					LabelText.Visibility = Visibility.Collapsed;
					m_editStartText = LabelText.Text;
					AssetTextBox.Focus();
				}
				else
				{
					AssetTextBox.Visibility = Visibility.Collapsed;
					LabelText.Visibility = Visibility.Visible;
				}
			}
		}

		public static readonly DependencyProperty AlwaysHandleClickProperty = DependencyProperty.Register(nameof(AlwaysHandleClick), typeof(bool), typeof(EditableTextBlock));

		public bool AlwaysHandleClick
		{
			get { return (bool)GetValue(AlwaysHandleClickProperty); }
			set { SetValue(AlwaysHandleClickProperty, value); }
		}

		#endregion
		private string m_editStartText;
	}
}
