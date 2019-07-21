using System;
using System.Text.RegularExpressions;
using System.Windows;
using KlaxCore.EditorHelper;
using KlaxShared.Attributes;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for FloatInspector.xaml
	/// </summary>
	public partial class FloatInspector : BaseInspectorControl
	{
		public FloatInspector()
		{
			InitializeComponent();
		}

		public override void SetValueOnly(object value)
		{
			float floatValue = (float)value;

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

		private void InputBox_FocusChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			m_inspector.Lock(InputBox.TextBox.IsFocused);

			if (!m_bLocked)
			{
				if (!InputBox.TextBox.IsFocused)
				{
					if (float.TryParse(SanitizeString(InputBox.Text), out float result))
					{
						SetInspectorValue(PropertyInfo, (float)PropertyInfo.Value, result);
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
				if (float.TryParse(SanitizeString(InputBox.Text), out float result))
				{
					SetInspectorValue(PropertyInfo, (float)PropertyInfo.Value, result);
				}

				m_bLocked = true;
				m_inspector.Unfocus();
				m_bLocked = false;
			}
		}

		private static string SanitizeString(string input)
		{
			return input.Replace('.', ',');
		}

		private static bool IsTextAllowed(string text)
		{
			return !m_regex.IsMatch(text);
		}

		private void InputBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private float GetAbsoluteDragModifier(float currentValue)
		{
			return (Math.Max(Math.Abs(currentValue) * FloatInspectorDragMultiplier, 0.001f));
		}

		private void InputBoxX_IncreaseValue(NumericTextBoxInputField element)
		{
			if (float.TryParse(element.TextBox.Text, out float result))
			{
				result += GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				SetInspectorValue(PropertyInfo, (float)PropertyInfo.Value, result);
			}
		}

		private void InputBoxX_DecreaseValue(NumericTextBoxInputField element)
		{
			if (float.TryParse(element.TextBox.Text, out float result))
			{
				result -= GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				SetInspectorValue(PropertyInfo, (float)PropertyInfo.Value, result);
			}
		}

		private void InputBoxY_ValueDragStart(NumericTextBoxInputField element)
		{
			m_inspector.Lock(true);
		}

		private void InputBoxY_ValueDragStop(NumericTextBoxInputField element)
		{
			m_inspector.Lock(false);
		}

		//Only allow numerical input
		private static readonly Regex m_regex = new Regex("[^0-9.,-]+");
		private bool m_bLocked;
	}
}
