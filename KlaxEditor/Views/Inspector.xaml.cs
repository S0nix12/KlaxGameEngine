using KlaxCore.GameFramework;
using KlaxEditor.UserControls;
using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.ViewModels.EditorWindows;
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
using KlaxCore.EditorHelper;
using KlaxEditor.Annotations;
using DragDropHelpers = KlaxEditor.Utility.DragDropHelpers;

namespace KlaxEditor.Views
{

	/// <summary>
	/// Interaction logic for Inspector.xaml
	/// </summary>
	public partial class Inspector : UserControl, IInspectorView
	{
		public Inspector()
		{
			InitializeComponent();

			MainGrid.Visibility = Visibility.Hidden;
		}

		private void AddComponentToInspectedEntity(Type type)
		{
			if (DataContext is CInspectorViewModel vm)
			{
				if (vm.AddComponentCommand.CanExecute(type))
				{
					vm.AddComponentCommand.Execute(type);
				}
			}
		}

		private bool m_updateQueued;
		private List<CObjectBase> m_updateList;
		private object m_updateListLock = new object();
		public void ShowInspectors(List<CObjectBase> propertyInfo)
		{
			if (m_updateQueued)
			{
				lock (m_updateListLock)
				{
					m_updateList = propertyInfo;
				}
				return;
			}

			m_updateQueued = true;
			m_updateList = propertyInfo;

			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (Action)(() =>
			{
				lock (m_updateListLock)
				{
					PropertyInspector.ShowInspectors(m_updateList);
					m_updateQueued = false;
				}
			}));
		}

		public void ClearInspector()
		{
			PropertyInspector.ClearInspector();
		}

		public void LockInspector(bool bLocked)
		{
			PropertyInspector.Lock(bLocked);
		}

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeView view = sender as TreeView;

			if (e.NewValue != null)
			{
				TreeViewItem tvitem = view.ItemContainerGenerator.ContainerFromItem(view.SelectedItem) as TreeViewItem;
				if (tvitem != null)
				{
					tvitem.BringIntoView();
				}
			}
		}
		private void EntityNameBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				(sender as TextBox).Text = m_originalEntityName;
				MainGrid.Focus();
			}
			else if (e.Key == Key.Return || e.Key == Key.Enter)
			{
				m_originalEntityName = (sender as TextBox).Text;

				ICommand command = (DataContext as CInspectorViewModel).RenameEntityCommand;
				if (command.CanExecute(m_originalEntityName))
				{
					command.Execute(m_originalEntityName);
				}

				MainGrid.Focus();
			}
		}

		private void EntityNameBox_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox box = sender as TextBox;

			m_originalEntityName = box.Text;
			box.SelectAll();
		}

		private void EntityNameBox_LostFocus(object sender, RoutedEventArgs e)
		{
			(sender as TextBox).Text = m_originalEntityName;
		}

		public void SetEntityName(string name)
		{
			EntityNameBox.Text = name;
		}

		public void SetInspectorVisible(bool bActive)
		{
			MainGrid.Visibility = bActive ? Visibility.Visible : Visibility.Hidden;
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 10);
			e.Handled = true;
		}

		private void SceneComponentEntry_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			m_sceneComponentDragStartPoint = e.GetPosition(null);
			if (sender is FrameworkElement draggedElement)
			{
				m_draggedData = new DataObject("sceneComponent", draggedElement.DataContext);
				m_draggedElement = draggedElement;
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void OnMouseDragUp(object sender, Point args)
		{
			m_draggedElement = null;
			m_draggedData = null;
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
				DragDrop.DoDragDrop(m_draggedElement, m_draggedData, DragDropEffects.Move);
			}
		}

		private string m_originalEntityName;

		private FrameworkElement m_draggedElement;
		private DataObject m_draggedData;
		private Point m_sceneComponentDragStartPoint;

	}
}
