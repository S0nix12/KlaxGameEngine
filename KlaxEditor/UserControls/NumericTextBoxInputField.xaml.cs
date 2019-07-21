using KlaxIO.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace KlaxEditor.UserControls
{
	/// <summary>
	/// Interaction logic for NumericTextBoxInputField.xaml
	/// </summary>
	public partial class NumericTextBoxInputField : UserControl
	{
		[DllImport("User32.dll")]
		private static extern bool SetCursorPos(int X, int Y);

		public NumericTextBoxInputField()
		{
			InitializeComponent();

			DragArea.Visibility = ShowDragArea ? Visibility.Visible : Visibility.Collapsed;
			DragAreaColumn.Width = ShowDragArea ? new GridLength(20.0) : new GridLength();
		}

		private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Input.SetCursorVisibility(false);

			m_initialMousePosition = System.Windows.Forms.Cursor.Position;
			MouseHook.OnMouseUp += MouseButtonUp;

			m_originalRect = System.Windows.Forms.Cursor.Clip;
			System.Drawing.Point dragAreaPivot = new System.Drawing.Point(m_initialMousePosition.X - DRAGAREA_HALF_WIDTH, m_initialMousePosition.Y);
			System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(dragAreaPivot, new System.Drawing.Size(DRAGAREA_HALF_WIDTH * 2 + 1, 1));

			m_timer.Interval = TimeSpan.FromMilliseconds(1);
			m_timer.Tick += (obj, args) =>
			{
				CheckMouse();
			};
			m_timer.Start();

			e.Handled = true;

			ValueDragStart?.Invoke(this);
		}

		private void MouseButtonUp(object sender, Point args)
		{
			MouseHook.OnMouseUp -= MouseButtonUp;
			m_timer.Stop();

			Input.SetCursorVisibility(true);
			System.Windows.Forms.Cursor.Clip = m_originalRect;

			ValueDragStop?.Invoke(this);
		}

		private void CheckMouse()
		{
			var mousePosition = System.Windows.Forms.Cursor.Position;
			int deltaX = mousePosition.X - m_initialMousePosition.X;

			if (deltaX < 0)
			{
				DecreaseValue?.Invoke(this);
			}
			else if (deltaX > 0)
			{
				IncreaseValue?.Invoke(this);
			}

			SetCursorPos(m_initialMousePosition.X, m_initialMousePosition.Y);
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			OnKeyDown?.Invoke(this, e);
		}

		private void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			OnGotFocus?.Invoke(this, e);
		}

		private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			OnLostFocus?.Invoke(this, e);
		}

		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			OnPreviewTextInput?.Invoke(this, e);
		}

		public string Text
		{
			get { return TextBox.Text; }
			set { TextBox.Text = value; }
		}

		public bool ShowDragArea
		{
			get { return (bool)GetValue(ShowDragAreaProperty); }
			set { SetValue(ShowDragAreaProperty, value); }
		}

		private System.Drawing.Rectangle m_originalRect;
		private System.Drawing.Point m_initialMousePosition;
		private DispatcherTimer m_timer = new DispatcherTimer();

		public event Action<NumericTextBoxInputField> IncreaseValue;
		public event Action<NumericTextBoxInputField> DecreaseValue;
		public event Action<NumericTextBoxInputField> ValueDragStart;
		public event Action<NumericTextBoxInputField> ValueDragStop;

		public new event EventHandler<KeyEventArgs> OnKeyDown;
		public new event EventHandler<RoutedEventArgs> OnGotFocus;
		public new event EventHandler<RoutedEventArgs> OnLostFocus;
		public new event EventHandler<TextCompositionEventArgs> OnPreviewTextInput;


		public static DependencyProperty ShowDragAreaProperty = DependencyProperty.Register(nameof(ShowDragArea), typeof(bool), typeof(NumericTextBoxInputField), new PropertyMetadata(true, new PropertyChangedCallback(ShowDragAreaPropertyChanged)));


		private static void ShowDragAreaPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is NumericTextBoxInputField control)
			{
				bool newValue = (bool)e.NewValue;
				control.DragArea.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
				control.DragAreaColumn.Width = newValue ? new GridLength(20.0) : new GridLength();
			}
		}

		private const int DRAGAREA_HALF_WIDTH = 5;
	}
}
