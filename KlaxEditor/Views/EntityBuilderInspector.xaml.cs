using KlaxCore.EditorHelper;
using KlaxEditor.UserControls.InspectorControls;
using KlaxEditor.ViewModels.EditorWindows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Controls;
using DragDropHelpers = KlaxEditor.Utility.DragDropHelpers;

namespace KlaxEditor.Views
{
	/// <summary>
	/// Interaction logic for EntityBuilderInspector.xaml
	/// </summary>
	public partial class EntityBuilderInspector : UserControl, IInspectorView
	{
		public EntityBuilderInspector()
		{
			InitializeComponent();

			MainGrid.Visibility = Visibility.Hidden;
		}

		private void AddComponentToInspectedEntity(Type type)
		{
			if (DataContext is CEntityBuilderInspectorViewModel vm)
			{
				if (vm.AddComponentCommand.CanExecute(type))
				{
					vm.AddComponentCommand.Execute(type);
				}
			}
		}

		public void ShowInspectors(List<CObjectBase> propertyInfo)
		{
			PropertyInspector.ShowInspectors(propertyInfo);
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

				ICommand command = (DataContext as CEntityBuilderInspectorViewModel).RenameEntityCommand;
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
			m_dragStartPoint = e.GetPosition(null);
			if (sender is FrameworkElement draggedElement)
			{
				m_draggedData = new DataObject("builderSceneComponent", draggedElement.DataContext);
				m_draggedElement = draggedElement;
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void EntityComponentEntry_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			m_dragStartPoint = e.GetPosition(null);
			if (sender is FrameworkElement draggedElement)
			{
				m_draggedData = new DataObject("builderEntityComponent", draggedElement.DataContext);
				m_draggedElement = draggedElement;
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void KlaxVariableEntry_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			m_dragStartPoint = e.GetPosition(null);
			if (sender is FrameworkElement draggedElement)
			{
				m_draggedData = new DataObject("klaxVariableEntry", draggedElement.DataContext);
				m_draggedElement = draggedElement;
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void OnMouseDragUp(object sender, Point args)
		{
			m_bIsInDragDrop = false;
			m_draggedElement = null;
			m_draggedData = null;
			MouseHook.OnMouseUp -= OnMouseDragUp;
		}

		private void OnPreviewMouseMoveDrag(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed || m_draggedData == null || m_draggedElement == null || m_bIsInDragDrop)
			{
				return;
			}

			// Get the current mouse position
			Point mousePos = e.GetPosition(null);
			Vector diff = m_dragStartPoint - mousePos;

			if (DragDropHelpers.IsMovementBiggerThreshold(diff))
			{
				m_bIsInDragDrop = true;
				// Initialize the drag & drop operation
				DragDrop.DoDragDrop(m_draggedElement, m_draggedData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
			}
		}
		private void AddGraphPopup_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			Popup senderPopup = (Popup)sender;
			if (senderPopup != null)
			{
				senderPopup.IsOpen = false;
			}
		}

		private string m_originalEntityName;

		private FrameworkElement m_draggedElement;
		private DataObject m_draggedData;
		private Point m_dragStartPoint;
		private bool m_bIsInDragDrop;
	}
}
