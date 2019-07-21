using KlaxEditor.UserControls;
using KlaxEditor.Utility;
using KlaxEditor.ViewModels;
using KlaxEditor.ViewModels.EditorWindows.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KlaxEditor.Views
{
	/// <summary>
	/// Interaction logic for WorldOutliner.xaml
	/// </summary>
	public partial class WorldOutliner : UserControl, IOutlinerView
	{
		public WorldOutliner()
		{
			InitializeComponent();
		}

		public void ScrollToEntity(int index)
		{
			VirtualizingPanel panel = EditorControlUtility.FindVisualChild<VirtualizingPanel>(EntityTree, "");

			if (panel != null)
			{
				panel.BringIndexIntoViewPublic(index);
			}
		}

		private void SceneComponentEntry_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			m_sceneComponentDragStartPoint = e.GetPosition(null);
			if (sender is FrameworkElement draggedElement)
			{
				m_draggedData = new DataObject("entity", draggedElement.DataContext);
				m_draggedElement = draggedElement;
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void OnMouseDragUp(object sender, Point args)
		{
			m_draggedData = null;
			m_draggedElement = null;
			MouseHook.OnMouseUp -= OnMouseDragUp;
		}

		private void OnPreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed || m_draggedData == null || m_draggedElement == null)
			{
				return;
			}

			// Get the current mouse position
			Point mousePos = e.GetPosition(null);
			Vector diff = m_sceneComponentDragStartPoint - mousePos;

			if (DragDropHelpers.IsMovementBiggerThreshold(diff))
			{
				// Initialize the drag & drop operation
				DragDrop.DoDragDrop(m_draggedElement, m_draggedData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
			}
		}

		private DependencyObject m_draggedElement;
		private DataObject m_draggedData;
		private Point m_sceneComponentDragStartPoint;
	}
}
