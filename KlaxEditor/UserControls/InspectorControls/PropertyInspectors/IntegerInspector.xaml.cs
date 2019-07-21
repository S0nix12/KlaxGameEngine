using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using KlaxCore.EditorHelper;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for FloatInspector.xaml
	/// </summary>
	public partial class IntegerInspector : BaseInspectorControl
	{
		public IntegerInspector()
		{
			InitializeComponent();
		}

		public override void SetValueOnly(object value)
		{
			m_bLocked = true;
			InputBox.Text = value.ToString();
			m_bLocked = false;
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			m_bLocked = true;
			InputBox.Text = info.Value.ToString();
			m_bLocked = false;
		}

		private void InputBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			m_inspector.Lock(true);
		}

		private void InputBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			m_inspector.Lock(false);
			if (!m_bLocked)
			{
				if (!InputBox.TextBox.IsFocused)
				{
					if (int.TryParse(InputBox.Text, out int result))
					{
						SetInspectorValue(PropertyInfo, (int)PropertyInfo.Value, result);
					}
				}
			}
		}

		private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				m_inspector.Unfocus();
			}
			else if (e.Key == System.Windows.Input.Key.Enter)
			{
				if (int.TryParse(InputBox.Text, out int result))
				{
					SetInspectorValue(PropertyInfo, (int)PropertyInfo.Value, result);
				}

				m_bLocked = true;
				m_inspector.Unfocus();
				m_bLocked = false;
			}
		}

		private static bool IsTextAllowed(string text)
		{
			return !m_regex.IsMatch(text);
		}

		private void InputBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private void InputBoxX_IncreaseValue(NumericTextBoxInputField element)
		{
			if (int.TryParse(element.TextBox.Text, out int result))
			{
				result += GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				SetInspectorValue(PropertyInfo, (int)PropertyInfo.Value, result);
			}
		}

		private void InputBoxX_DecreaseValue(NumericTextBoxInputField element)
		{
			if (int.TryParse(element.TextBox.Text, out int result))
			{
				result -= GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				SetInspectorValue(PropertyInfo, (int)PropertyInfo.Value, result);
			}
		}

		private int GetAbsoluteDragModifier(float currentValue)
		{
			float result = (Math.Max(Math.Abs(currentValue) * IntegerInspectorDragMultiplier, 0.001f));

			return Math.Max((int)result, 1);
		}

		private void InputBoxY_ValueDragStart(NumericTextBoxInputField element)
		{
			m_inspector.Lock(true);
		}

		private void InputBoxY_ValueDragStop(NumericTextBoxInputField element)
		{
			m_inspector.Lock(false);
		}

		private bool m_bLocked;
		private static readonly Regex m_regex = new Regex("[^0-9-]+");
	}
}
