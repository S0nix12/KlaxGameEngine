using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KlaxEditor.UserControls;
using KlaxEditor.UserControls.AssetBrowser;
using KlaxEditor.Utility;

namespace KlaxEditor.Views
{
    /// <summary>
    /// Interaction logic for AssetBrowserView.xaml
    /// </summary>
    public partial class AssetBrowserView : UserControl
    {
        public AssetBrowserView()
        {
            InitializeComponent();
        }

		public bool IsInEditMode { get; set; }

		private void AssetEntryOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (IsInEditMode)
			{
				return;
			}
			if (sender is FrameworkElement element)
			{
				m_draggedItem = element;
				m_dragStartPos = e.GetPosition(null);
				int selectedItemCount = AssetSelectionViewer.SelectedItems.Count;
				if (selectedItemCount > 1)
				{
					// Drag multiple assets
					object[] draggedAssets = new object[selectedItemCount];
					for (int i = 0; i < selectedItemCount; i++)
					{
						draggedAssets[i] = AssetSelectionViewer.SelectedItems[i];
					}
					m_draggedData = new DataObject("assetEntries", draggedAssets);
				}
				else
				{
					// Drag single asset
					m_draggedData = new DataObject("assetEntry", element.DataContext);
				}
			}

			MouseHook.OnMouseUp += OnMouseDragUp;
		}

		private void FolderEntryOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (IsInEditMode)
			{
				return;
			}
			if (sender is FrameworkElement element)
			{
				m_draggedItem = element;
				m_draggedData = new DataObject("folderEntry", element.DataContext);
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
				DragDrop.DoDragDrop(m_draggedItem, m_draggedData, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Scroll);
			}
		}

		private void OnMouseDragUp(object sender, Point args)
		{
			m_draggedData = null;
			MouseHook.OnMouseUp -= OnMouseDragUp;
		}

		private DependencyObject m_draggedItem;
		private Point m_dragStartPos;
		private DataObject m_draggedData;

		private void CreateMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			CreateButton.IsOpen = false;
		}
	}
}
