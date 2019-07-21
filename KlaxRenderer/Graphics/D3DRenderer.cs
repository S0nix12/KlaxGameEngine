using System;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX;

using Device = SharpDX.Direct3D11.Device;
using KlaxShared.Attributes;
using KlaxRenderer.Graphics.UI;
using KlaxConfig;
using KlaxRenderer.Scene;

namespace KlaxRenderer.Graphics
{
    class CD3DRenderer : IDisposable
    {
        public bool Wireframe { get; set; }
		public int MSAASampleCount { get; set; } = 4;
        public int SleepMilliseconds { get; private set; }

        public CD3DRenderer()
        {
            Configuration.EnableObjectTracking = false;
            Configuration.EnableTrackingReleaseOnFinalizer = true;
            ComObject.LogMemoryLeakWarning = (warning) => System.Diagnostics.Trace.WriteLine(warning);

            m_uiRenderer = new ImGuiRenderer();
        }

        public void Init()
        {
			InitializeDevice();
            m_uiRenderer.Init(m_d3dDevice, m_d3dDeviceContext, 0, 0, 0, 0);
        }

		public void BeginFrame(float deltaTime)
		{
			m_currentFrameTime = deltaTime;
		}

		public void SetActiveWindow(CWindowRenderer window)
		{
			ActiveWindow = window;

			m_uiRenderer.SetContext(window.UIContext);
			m_uiRenderer.Resize(ActiveWindow.Width, ActiveWindow.Height, ActiveWindow.Left, ActiveWindow.Top);
			m_uiRenderer.BeginRender(m_currentFrameTime);

			m_d3dDeviceContext.OutputMerger.SetDepthStencilState(ActiveWindow.DepthStencilState);
			m_d3dDeviceContext.OutputMerger.SetRenderTargets(ActiveWindow.DepthStencilView, ActiveWindow.RenderTargetView);
			m_d3dDeviceContext.Rasterizer.State = ActiveWindow.RasterizerState;
			m_d3dDeviceContext.Rasterizer.SetViewport(ActiveWindow.Viewport);
			m_d3dDeviceContext.ClearRenderTargetView(ActiveWindow.RenderTargetView, ClearColor);
			m_d3dDeviceContext.ClearDepthStencilView(ActiveWindow.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0xFF);
		}
		public void PresentActiveWindow()
		{
			m_uiRenderer.EndRender();
			ActiveWindow.Render();
		}

		public void EndFrame()
		{
			m_uiRenderer.EndFrame();
		}

		public void Dispose()
        {
            m_d3dDevice.Dispose();
            m_d3dDeviceContext.Dispose();
            m_uiRenderer.Dispose();
        }

		private void InitializeDevice()
		{
			m_d3dDevice = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
			m_d3dDeviceContext = m_d3dDevice.ImmediateContext;
		}

		public static SharpDX.Color ClearColor
		{ get; set; } = new SharpDX.Color(0, 0, 0);

		[CVar()]
        public static int EnableVsync
        { get; set; } = 0;

        public Device D3DDevice
        { get { return m_d3dDevice; } }

        public DeviceContext D3DDeviceContext
        { get { return m_d3dDeviceContext; } }

		public ImGuiRenderer UIRenderer
		{ get { return m_uiRenderer;} }

		public CWindowRenderer ActiveWindow { get; private set; }
        private Device m_d3dDevice;
        private DeviceContext m_d3dDeviceContext;
        private ImGuiRenderer m_uiRenderer;
		private float m_currentFrameTime;
	}
}
