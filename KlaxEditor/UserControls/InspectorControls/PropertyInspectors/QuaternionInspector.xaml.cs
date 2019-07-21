using KlaxMath;
using SharpDX;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KlaxCore.EditorHelper;

namespace KlaxEditor.UserControls.InspectorControls
{
	/// <summary>
	/// Interaction logic for QuaternionInspector.xaml
	/// </summary>
	public partial class QuaternionInspector : BaseInspectorControl
	{
		public QuaternionInspector()
		{
			InitializeComponent();
		}

		public override void SetValueOnly(object value)
		{
			UpdateDisplay((Quaternion)value);
		}

		public override void PropertyInfoChanged(CObjectProperty info)
		{
			base.PropertyInfoChanged(info);

			UpdateDisplay((Quaternion)info.Value);
		}

		private void UpdateDisplay(Quaternion value)
		{
			m_bLocked = true;

			Vector3 euler = value.ToEuler();

			InputBoxX.Text = MathUtil.RadiansToDegrees(euler.X).ToString();
			InputBoxY.Text = MathUtil.RadiansToDegrees(euler.Y).ToString();
			InputBoxZ.Text = MathUtil.RadiansToDegrees(euler.Z).ToString();

			m_bLocked = false;
		}

		private void UpdateValueFromTextbox()
		{
			if (float.TryParse(SanitizeString(InputBoxX.Text), out float resultX))
			{
				if (float.TryParse(SanitizeString(InputBoxY.Text), out float resultY))
				{
					if (float.TryParse(SanitizeString(InputBoxZ.Text), out float resultZ))
					{
						Vector3 euler = new Vector3(MathUtil.DegreesToRadians(resultX), MathUtil.DegreesToRadians(resultY), MathUtil.DegreesToRadians(resultZ));
						SetInspectorValue(PropertyInfo, (Quaternion)PropertyInfo.Value, euler.EulerToQuaternion());
					}
				}
			}
		}

		private void InputBox_FocusChanged(object sender, RoutedEventArgs e)
		{
			NumericTextBoxInputField inputBox = sender as NumericTextBoxInputField;
			m_inspector.Lock(inputBox.TextBox.IsFocused);

			if (!m_bLocked)
			{
				if (!inputBox.IsFocused)
				{
					UpdateValueFromTextbox();
				}
			}
		}

		private void InputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				m_inspector.Unfocus();
			}
			else if (e.Key == Key.Enter)
			{
				UpdateValueFromTextbox();

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

		private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		private float GetAbsoluteDragModifier(float currentValue)
		{
			return 1.0f;
		}

		private void InputBoxX_IncreaseValue(NumericTextBoxInputField element)
		{
			if (float.TryParse(element.TextBox.Text, out float result))
			{
				result += GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				UpdateValueFromTextbox();
			}
		}

		private void InputBoxX_DecreaseValue(NumericTextBoxInputField element)
		{
			if (float.TryParse(element.TextBox.Text, out float result))
			{
				result -= GetAbsoluteDragModifier(result);
				element.TextBox.Text = result.ToString();

				UpdateValueFromTextbox();
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
