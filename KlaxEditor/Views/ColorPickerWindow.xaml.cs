using SharpDX;
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
using System.Windows.Shapes;

namespace KlaxEditor.Views
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow(System.Windows.Media.Color color)
        {
            InitializeComponent();

            Loaded += (obj, args) =>
            {
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                var mouse = transform.Transform(GetMousePosition());
                Left = mouse.X - ActualWidth;
                Top = mouse.Y - ActualHeight;
            };

            ColorPicker.SelectedColor = color;
        }

        private System.Windows.Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new System.Windows.Point(point.X, point.Y);
        }

        public event Action<System.Windows.Media.Color> PreviewColorChanged;

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                PreviewColorChanged?.Invoke(e.NewValue.Value);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
