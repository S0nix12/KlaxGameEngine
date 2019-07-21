using KlaxEditor.Views;
using SharpDX;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KlaxCore.EditorHelper;
using KlaxEditor.Utility;

namespace KlaxEditor.UserControls.InspectorControls
{
    /// <summary>
    /// Interaction logic for ColorInspector.xaml
    /// </summary>
    public partial class ColorInspector : BaseInspectorControl
    {
        public ColorInspector()
        {
            InitializeComponent();
        }

		public override void SetValueOnly(object value)
		{
			Color4 color = ToColor4(PropertyInfo.Value);
			ColorButton.Background = new SolidColorBrush(EditorConversionUtility.ConvertEngineColorToSystem(color));
		}

		public override void PropertyInfoChanged(CObjectProperty info)
        {
            base.PropertyInfoChanged(info);

			Color4 color = ToColor4(PropertyInfo.Value);
            ColorButton.Background = new SolidColorBrush(EditorConversionUtility.ConvertEngineColorToSystem(color));
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
		{
			Color4 originalColor = ToColor4(PropertyInfo.Value);

			ColorPickerWindow window = new ColorPickerWindow(EditorConversionUtility.ConvertEngineColorToSystem(originalColor));
            window.PreviewColorChanged += (color) =>
            {
                SetInspectorValue(PropertyInfo, ToColor4(PropertyInfo.Value), EditorConversionUtility.ConvertSystemColorToEngine(color), false);
            };

            bool? result = window.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                SetInspectorValue(PropertyInfo, ToColor4(PropertyInfo.Value), originalColor, false);
            }
            else if (result.HasValue && result.Value)
            {
                SetInspectorValue(PropertyInfo, originalColor, ToColor4(PropertyInfo.Value), true);
            }
        }

		private Color4 ToColor4(object value)
		{
			return (Color4?) value ?? new Color4();
		}
    }
}
