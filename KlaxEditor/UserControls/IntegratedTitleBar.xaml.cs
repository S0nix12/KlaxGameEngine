using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KlaxEditor.UserControls
{
	public partial class IntegratedTitleBar : UserControl
	{
		public IntegratedTitleBar()
		{
			InitializeComponent();

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, EventArgs args)
		{
			Window = Window.GetWindow(this);
			if (Window != null)
			{
				Window.StateChanged += OnWindowStateChanged;
				UpdateMaximizeRestoreButton();
			}

			MaxButton.Visibility = CanMaximize ? Visibility.Visible : Visibility.Collapsed;
			MinButton.Visibility = CanMinimize ? Visibility.Visible : Visibility.Collapsed;
			ApplicationIcon.Visibility = DisplayApplicationIcon ? Visibility.Visible : Visibility.Collapsed;
		}

		private void UpdateMaximizeRestoreButton()
		{
			if (Window != null)
			{
				switch (Window.WindowState)
				{
					case WindowState.Normal:
						RestoreButton.Visibility = Visibility.Collapsed;
						MaximizeButton.Visibility = Visibility.Visible;
						break;
					case WindowState.Maximized:
						RestoreButton.Visibility = Visibility.Visible;
						MaximizeButton.Visibility = Visibility.Collapsed;
						break;
				}
			}
		}

		private void OnWindowStateChanged(object sender, EventArgs args)
		{
			UpdateMaximizeRestoreButton();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Window?.Close();
		}

		private void MaxButton_Click(object sender, RoutedEventArgs e)
		{
			if (Window != null)
			{
				switch (Window.WindowState)
				{
					case WindowState.Normal:
						Window.WindowState = WindowState.Maximized;
						break;
					case WindowState.Maximized:
						Window.WindowState = WindowState.Normal;
						break;
				}
			}
		}

		private void MinButton_Click(object sender, RoutedEventArgs e)
		{
			if (Window != null)
			{
				Window.WindowState = WindowState.Minimized;
			}
		}

		private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2 && CanMaximize)
			{
				if (Window.WindowState == WindowState.Normal)
				{
					Window.WindowState = WindowState.Maximized;
				}
				else if (Window.WindowState == WindowState.Maximized)
				{
					Window.WindowState = WindowState.Normal;
				}

				MouseHook.OnMouseMove -= OnMouseMove;
				MouseHook.OnMouseUp -= OnMouseUp;
			}
			else
			{
				m_initialMousePoint = e.GetPosition(null);
				MouseHook.OnMouseMove += OnMouseMove;
				MouseHook.OnMouseUp += OnMouseUp;
			}
		}

		private void OnMouseUp(object sender, Point point)
		{
			MouseHook.OnMouseMove -= OnMouseMove;
		}

		private void OnMouseMove(object sender, Point newPoint)
		{
			if (Window.WindowState == WindowState.Maximized)
			{
				PrepareMaximizedDrag();
			}

			Window.DragMove();
		}

		private void PrepareMaximizedDrag()
		{
			double previousWidth = Window.ActualWidth;
			Window.WindowState = WindowState.Normal;

			System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
			double horizontalPercentage = Mouse.GetPosition(Window).X / previousWidth;
			Window.Top = 0;
			Window.Left = point.X - Window.ActualWidth * horizontalPercentage;
		}

		public Window Window { get; set; } = null;
		public bool CanMaximize { get; set; } = true;
		public bool CanMinimize { get; set; } = true;
		public bool DisplayApplicationIcon { get; set; } = false;

		private Point m_initialMousePoint;

		private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			LogUtility.Log("(call)");
		}
	}
}
