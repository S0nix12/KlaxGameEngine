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
using KlaxCore.EditorHelper;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for StringInspector.xaml
	/// </summary>
	public partial class StringInspector : BaseInspectorControl
	{
		public StringInspector()
		{
			InitializeComponent();
		}

		public override void SetValueOnly(object value)
		{
			InputBox.Text = value != null ? value.ToString() : "";
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			InputBox.Text = info.Value != null ? info.Value.ToString() : "";
		}

		private void InputBox_FocusChanged(object sender, RoutedEventArgs e)
		{
			m_inspector.Lock(InputBox.TextBox.IsFocused);

			if (!m_bLocked)
			{
				if (!InputBox.TextBox.IsFocused)
				{
					string oldValue = PropertyInfo.Value as string;
					SetInspectorValue(PropertyInfo, oldValue ?? "", InputBox.Text);
				}
			}
		}

		private void InputBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				m_inspector.Unfocus();
			}
			else if (e.Key == Key.Enter)
			{
				string oldValue = PropertyInfo.Value as string;
				SetInspectorValue(PropertyInfo, oldValue ?? "", InputBox.Text);

				m_bLocked = true;
				m_inspector.Unfocus();
				m_bLocked = false;
			}
		}

		private bool m_bLocked;
	}
}
