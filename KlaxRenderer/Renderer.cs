using KlaxRenderer.Graphics;
using System;
using System.Collections.Generic;
using KlaxShared;
using Vector4 = SharpDX.Vector4;
using KlaxShared.Containers;
using System.Threading;
using System.Threading.Tasks;
using KlaxRenderer.Graphics.ResourceManagement;
using KlaxRenderer.Lights;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using KlaxShared.Attributes;

namespace KlaxRenderer
{
	public enum ERendererDispatcherPriority
	{
		BeginFrame,
		EndFrame
	}

	public class CRenderer : IDisposable, IDispatchable<ERendererDispatcherPriority>
	{
		public CRenderer()
		{
			m_cvars = new CRendererCVars(this);
		}

		public void Init(IRenderSurface surface)
		{
			m_owningThread = Thread.CurrentThread;
			m_d3dRenderer.Init();

			// Create the default scene
			CreateRenderScene(surface);
			ActiveScene = m_renderScenes[0];

			Init();
		}

		private void Init()
		{
			ResourceManager = new CResourceManager(m_d3dRenderer.D3DDevice);
			ResourceManager.InitDefaultResources(m_d3dRenderer.D3DDeviceContext);
			Initialized = true;
			m_modelLoader = new ModelLoader(m_d3dRenderer.D3DDevice);
			m_cvars.InitConsole();
		}

		public CRenderScene CreateRenderScene(IRenderSurface surface)
		{
			foreach (CRenderScene scene in m_renderScenes)
			{
				if (scene.SceneRenderer.Hwnd == surface.GetHWND())
				{
					return null;
				}
			}
			CRenderScene newScene = new CRenderScene();
			newScene.InitScene(m_d3dRenderer.D3DDevice, m_d3dRenderer.D3DDeviceContext, surface);
			newScene.DebugRenderer.Init(m_d3dRenderer.D3DDevice, m_d3dRenderer.UIRenderer.FontProvider);
			newScene.SceneRenderer.UIContext = m_d3dRenderer.UIRenderer.CreateAdditionalContext();
			m_renderScenes.Add(newScene);
			return newScene;
		}

		public void BeginFrame(float deltaTime)
		{
			if (!Initialized)
			{
				return;
			}

			ResourceManager.UpdateResources(m_d3dRenderer.D3DDeviceContext);
			ActiveScene = m_renderScenes[0];
			m_d3dRenderer.BeginFrame(deltaTime);
			m_d3dRenderer.SetActiveWindow(ActiveScene.SceneRenderer);
		}

		public void RenderFrame(float deltaTime)
		{
			if (!Initialized)
			{
				return;
			}

			DoFrame(deltaTime);
			ActiveScene.UpdateScene(deltaTime);
			ActiveScene.RenderScene(m_d3dRenderer.D3DDevice, m_d3dRenderer.D3DDeviceContext);
			m_d3dRenderer.PresentActiveWindow();
			for (int i = 1; i < m_renderScenes.Count; i++)
			{
				CRenderScene scene = m_renderScenes[i];
				m_d3dRenderer.SetActiveWindow(scene.SceneRenderer);
				scene.UpdateScene(deltaTime);
				scene.RenderScene(m_d3dRenderer.D3DDevice, m_d3dRenderer.D3DDeviceContext);
				m_d3dRenderer.PresentActiveWindow();
			}
			m_d3dRenderer.EndFrame();
		}

		public void Resize(int newWidth, int newHeight, int newLeft, int newTop, IntPtr windowHwnd)
		{
			foreach (CRenderScene scene in m_renderScenes)
			{
				if (scene.SceneRenderer.Hwnd == windowHwnd)
				{
					scene.SceneRenderer.Resize(newWidth, newHeight, newLeft, newTop, m_d3dRenderer.D3DDevice);
				}
			}
		}

		public void SetWireframeEnabled(bool bEnabled)
		{
			m_d3dRenderer.Wireframe = bEnabled;
			foreach (CRenderScene scene in m_renderScenes)
			{
				scene.SceneRenderer.InitializeRasterizer(m_d3dRenderer.D3DDevice);
			}
		}

		public void SetMSAASampleCount(int sampleCount)
		{
			m_d3dRenderer.MSAASampleCount = sampleCount;
			foreach (CRenderScene scene in m_renderScenes)
			{
				scene.SceneRenderer.InitializeDeviceResources(m_d3dRenderer.D3DDevice);
			}
		}

		private void DoFrame(float deltaTime)
		{
			m_dispatcherQueue.Execute((int)ERendererDispatcherPriority.BeginFrame);
		}

		public void Dispose()
		{
			m_d3dRenderer.Dispose();
			foreach (CRenderScene scene in m_renderScenes)
			{
				scene.Dispose();
			}
			m_cvars.Dispose();
			ResourceManager.Dispose();
			System.Diagnostics.Trace.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
		}

		public void Dispatch(ERendererDispatcherPriority priority, Action action)
		{
			m_dispatcherQueue.Add((int)priority, action);
		}

		public bool IsInAuthoritativeThread()
		{
			return Thread.CurrentThread == m_owningThread;
		}

		public static CRenderer Instance { get; } = new CRenderer();

		internal CResourceManager ResourceManager { get; private set; }

		public bool Initialized { get; private set; }
		public CRenderScene ActiveScene { get; private set; }
		private List<CRenderScene> m_renderScenes = new List<CRenderScene>();

		CD3DRenderer m_d3dRenderer = new CD3DRenderer();

		ModelLoader m_modelLoader;

		private CRendererCVars m_cvars;

		private Thread m_owningThread;
		private DispatcherQueue m_dispatcherQueue = new DispatcherQueue((int)ERendererDispatcherPriority.EndFrame + 1);

		/// <summary>
		/// Should we use a separate thread for the render, currently this has no effect the renderer will always be on the same thread as the engine, we only use this to prepare some logic
		/// </summary>
		[CVar()]
		public static int UseRenderThread { get; private set; } = 0;
	}
}
