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
    /// Interaction logic for Vector3Inspector.xaml
    /// </summary>
    public partial class Vector3Inspector : BaseInspectorControl
    {
        public Vector3Inspector()
        {
            InitializeComponent();
        }

		public override void SetValueOnly(object value)
		{
			UpdateDisplay((Vector3)value);
		}

		public override void PropertyInfoChanged(CObjectProperty info)
        {
            base.PropertyInfoChanged(info);

            UpdateDisplay((Vector3)info.Value);
        }

        private void UpdateDisplay(Vector3 value)
        {
            m_bLocked = true;
			InputBoxX.TextBox.Text = value.X.ToString();
			InputBoxY.TextBox.Text = value.Y.ToString();
			InputBoxZ.TextBox.Text = value.Z.ToString();
			m_bLocked = false;
        }

        private void UpdateValueFromTextbox()
        {
			if (float.TryParse(SanitizeString(InputBoxX.TextBox.Text), out float resultX))
			{
				if (float.TryParse(SanitizeString(InputBoxY.TextBox.Text), out float resultY))
				{
					if (float.TryParse(SanitizeString(InputBoxZ.TextBox.Text), out float resultZ))
					{
						SetInspectorValue(PropertyInfo, (Vector3)PropertyInfo.Value, new Vector3(resultX, resultY, resultZ));
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

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
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

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			SetInspectorValue(PropertyInfo, (Vector3)PropertyInfo.Value, new Vector3());
		}

		private void InputBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			NumericTextBoxInputField inputBox = sender as NumericTextBoxInputField;
			if (inputBox.IsFocused)
			{
				inputBox.TextBox.SelectAll();
			}
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
