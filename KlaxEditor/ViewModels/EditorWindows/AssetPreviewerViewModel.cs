using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KlaxEditor.Utility;
using KlaxEditor.Views;
using KlaxIO.Input;
using KlaxRenderer.Graphics;
using WpfSharpDxControl;

namespace KlaxEditor.ViewModels.EditorWindows
{
    class CAssetPreviewerViewModel : CEditorWindowViewModel
	{
		public CAssetPreviewerViewModel() : base("AssetPreview")
		{
			AssetPreviewerView view = new AssetPreviewerView();
			Content = view;
			m_viewPortControl = view.PreviewViewport;
			m_viewPortControl.Loaded += OnViewPortLoaded;
		}

		private void OnViewPortLoaded(object sender, RoutedEventArgs e)
		{
			if (m_bWorldLoaded && !PreviewScene.IsCreated)
			{
				IRenderSurface renderSurface = m_viewPortControl.GetRenderSurface();
				PreviewScene.EditorThread_CreateScene(renderSurface, EInputClass.AssetPreview);
				Input.SetInputClassActive(EInputClass.AssetPreview, IsActive);
			}
		}

		protected override void OnActiveChanged(bool bNewValue)
		{
			Input.SetInputClassActive(EInputClass.AssetPreview, bNewValue);
		}

		public override void PostWorldLoad()
		{
			base.PostWorldLoad();

			m_bWorldLoaded = true;
			if (m_viewPortControl.IsLoaded && !PreviewScene.IsCreated)
			{
				IRenderSurface renderSurface = m_viewPortControl.GetRenderSurface();
				PreviewScene.EditorThread_CreateScene(renderSurface, EInputClass.AssetPreview);
				Input.SetInputClassActive(EInputClass.AssetPreview, IsActive);
			}
		}

		public PreviewScene PreviewScene { get; private set; } = new PreviewScene();

		private readonly CRendererHostControl m_viewPortControl;
		private bool m_bWorldLoaded;
	}
}
