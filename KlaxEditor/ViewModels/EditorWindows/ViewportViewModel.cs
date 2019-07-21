using KlaxCore.Core;
using KlaxCore.GameFramework;
using KlaxEditor.ViewModels;
using KlaxRenderer.Graphics;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KlaxCore.EditorHelper;
using KlaxEditor.ViewModels.EditorWindows;
using KlaxRenderer;
using WpfSharpDxControl;
using KlaxEditor.Views;
using KlaxIO.Input;
using KlaxEditor.Utility;

namespace KlaxEditor.ViewModels
{
	class CViewportViewModel : CEditorWindowViewModel
	{
		public CViewportViewModel()
			: base("Scene Viewer")
		{
			SetIconSourcePath("Resources/Images/Tabs/viewport.png");

			var viewport = new Viewport();
			m_hostControl = viewport.RendererHostControl;
			m_hostControl.Loaded += OnHostControlLoaded;

			Content = viewport;
			DragEnterCommand = new CRelayCommand(OnDragEnter);
			DragOverCommand = new CRelayCommand(OnDragOver);
			DropCommand = new CRelayCommand(OnDrop);
			KeyDownCommand = new CRelayCommand(OnKeyDown);
		}

		private void OnHostControlLoaded(object sender, RoutedEventArgs e)
		{
			m_hostControl.Loaded -= OnHostControlLoaded;
			IRenderSurface renderSurface = m_hostControl.GetRenderSurface();

			if (!m_hostControl.IsFocused)
			{
				Input.SetInputClassActive(EInputClass.Default, false);
			}

			CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CRenderer.Instance.Init(renderSurface);
				CEngine.Instance.LoadWorld(null, (world) =>
				{
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
				 {
					 CWorkspace.Instance.PostWorldLoad();
				 }));
				});
			});
		}

		protected override void OnActiveChanged(bool bNewValue)
		{
			Input.SetInputClassActive(EInputClass.Default, bNewValue);
		}

		private void OnKeyDown(object e)
		{
			KeyEventArgs args = (KeyEventArgs)e;

			if (IsActive && args.Key == Key.Delete)
			{
				var outliner = CWorkspace.Instance.GetTool<CWorldOutlinerViewModel>();
				if (outliner.SelectedEntityViewModel != null)
				{
					EditorEntityUtility.DestroyEntity(outliner.SelectedEntityViewModel.EntityId);
				}
			}
		}

		public void LockMouseCursor()
		{
			IRenderSurface renderSurface = m_hostControl.GetRenderSurface();
			Rectangle clipRect = new Rectangle(renderSurface.GetLeft(), renderSurface.GetTop(), renderSurface.GetWidth(), renderSurface.GetHeight());
			System.Windows.Forms.Cursor.Clip = clipRect;
			Input.SetCursorVisibility(false);
		}

		public void FreeMouseCursor()
		{
			// Unhide and free the mouse cursor
			System.Windows.Forms.Cursor.Clip = new Rectangle();
			Input.SetCursorVisibility(true);
		}

		public override void PostWorldLoad()
		{
			base.PostWorldLoad();

			m_world = CEngine.Instance.CurrentWorld;

			CEngine engine = CEngine.Instance;
			engine.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
			{
				CEntity entity = m_world.SpawnEntity<CEntity>();
				entity.AddComponent<CSceneComponent>(true, true);
				m_world.DestroyEntity(entity);
			});

			m_hostControl.SetWorld(m_world);
		}

		private void OnDragEnter(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("assetEntry"))
			{
				CAssetEntryViewModel assetEntry = (CAssetEntryViewModel)args.Data.GetData("assetEntry");
				if (KlaxCore.EditorHelper.DragDropHelpers.CanHandleDragDrop(assetEntry.Asset))
				{
					args.Effects = DragDropEffects.Copy;
				}
			}

			args.Handled = true;
		}

		private void OnDragOver(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			args.Effects = DragDropEffects.None;
			if (args.Data.GetDataPresent("assetEntry"))
			{
				CAssetEntryViewModel assetEntry = (CAssetEntryViewModel)args.Data.GetData("assetEntry");
				if (KlaxCore.EditorHelper.DragDropHelpers.CanHandleDragDrop(assetEntry.Asset))
				{
					args.Effects = DragDropEffects.Copy;
				}
			}

			args.Handled = true;
		}

		private void OnDrop(object e)
		{
			DragEventArgs args = (DragEventArgs)e;
			if (args.Data.GetDataPresent("assetEntry"))
			{
				CAssetEntryViewModel assetEntry = (CAssetEntryViewModel)args.Data.GetData("assetEntry");
				CEngine.Instance.Dispatch(EEngineUpdatePriority.BeginFrame, () =>
				{
					KlaxCore.EditorHelper.DragDropHelpers.HandleDrop(assetEntry.Asset);
				});
			}
		}

		public ICommand DragEnterCommand { get; set; }
		public ICommand DragOverCommand { get; set; }
		public ICommand DropCommand { get; set; }
		public ICommand KeyDownCommand { get; set; }
		private CWorld m_world;
		private CRendererHostControl m_hostControl;
	}
}
