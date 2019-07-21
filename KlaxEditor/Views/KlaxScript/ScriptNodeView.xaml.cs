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
using KlaxEditor.Utility;
using Xceed.Wpf.AvalonDock.Controls;

namespace KlaxEditor.Views.KlaxScript
{
	/// <summary>
	/// Interaction logic for ScriptNodeView.xaml
	/// </summary>
	public partial class ScriptNodeView : UserControl
	{
		public ScriptNodeView()
		{
			InitializeComponent();
		}

		private void ExecutionPin_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is FrameworkElement element)
			{
				m_draggedItem = element;
				m_draggedData = new DataObject("executionPin", element.DataContext);
				m_dragStartPos = e.GetPosition(null);
			}
			
			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void InputPin_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is FrameworkElement element)
			{
				m_draggedItem = element;
				m_draggedData = new DataObject("inputPin", element.DataContext);
				m_dragStartPos = e.GetPosition(null);
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void OutputPin_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (sender is FrameworkElement element)
			{
				m_draggedItem = element;
				m_draggedData = new DataObject("outputPin", element.DataContext);
				m_dragStartPos = e.GetPosition(null);
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (m_draggedData == null || m_draggedItem == null || e.LeftButton != MouseButtonState.Pressed)
			{
				return;
			}
			
			Vector delta = e.GetPosition(null) - m_dragStartPos;
			if (DragDropHelpers.IsMovementBiggerThreshold(delta))
			{
				//DragDrop.DoDragDrop(m_draggedItem, m_draggedData, DragDropEffects.Link | DragDropEffects.Scroll);
			}
		}

		private void OnMouseDragUp(object sender, Point p)
		{
			m_draggedData = null;
			MouseHook.OnMouseUp -= OnMouseDragUp;
		}

		private DependencyObject m_draggedItem;
		private Point m_dragStartPos;
		private DataObject m_draggedData;

		private void NodeLeftMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Handled)
			{
				return;
			}

			this.FindVisualAncestor<NodeGraphView>()?.Focus();
		}
	}
}
