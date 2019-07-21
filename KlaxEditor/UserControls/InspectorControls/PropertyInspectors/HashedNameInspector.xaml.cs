using KlaxCore.EditorHelper;
using KlaxShared;
using System.Windows;
using System.Windows.Input;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for HashedNameInspector.xaml
	/// </summary>
	public partial class HashedNameInspector : BaseInspectorControl
	{
		public HashedNameInspector()
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
			m_inspector.Lock(InputBox.IsFocused);

			if (!m_bLocked)
			{
				if (!InputBox.IsFocused)
				{
					SHashedName oldValue = (SHashedName)PropertyInfo.Value;
					SHashedName newValue = new SHashedName(InputBox.Text);
					SetInspectorValue(PropertyInfo, oldValue, newValue);
					InputBox.Text = newValue.ToString();
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
				SHashedName oldValue = (SHashedName)PropertyInfo.Value;
				SHashedName newValue = new SHashedName(InputBox.Text);
				SetInspectorValue(PropertyInfo, oldValue, newValue);
				InputBox.Text = newValue.ToString();

				m_bLocked = true;
				m_inspector.Unfocus();
				m_bLocked = false;
			}
		}

		private bool m_bLocked;
	}
}
