using System;
using ImGuiNET;
using KlaxRenderer.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Scene
{
	class CWindowRenderer : IDisposable
	{
		public bool Wireframe { get; set; }
		public int MSAASampleCount { get; set; } = 4;

		public void Init(IRenderSurface surface, Device device)
		{
			Width = surface.GetWidth();
			Height = surface.GetHeight();
			Top = surface.GetTop();
			Left = surface.GetLeft();
			m_renderSurface = surface;
			
			InitializeDeviceResources(device);
		}

		public void Dispose()
		{
			SwapChain.Dispose();
			RenderTargetView.Dispose();
			DepthStencilState.Dispose();
			DepthStencilView.Dispose();
			DepthStencilTexture.Dispose();
			RasterizerState.Dispose();
		}

		public void Render()
		{
			SwapChain.Present(CD3DRenderer.EnableVsync, PresentFlags.None);
		}

		public void Resize(int newWidth, int newHeight, int newLeft, int newTop, Device device)
		{
			Width = newWidth;
			Height = newHeight;
			Left = newLeft;
			Top = newTop;

			RenderTargetView.Dispose();
			DepthStencilView.Dispose();
			DepthStencilTexture.Dispose();

			SwapChain.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

			using (Texture2D backBuffer = SwapChain.GetBackBuffer<Texture2D>(0))
			{
				RenderTargetView = new RenderTargetView(device, backBuffer);
			}

			CreateDepthStencilView(Width, Height, device);
			Viewport = new Viewport(0, 0, Width, Height, 0.0f, 1.0f);
		}

		public void InitializeDeviceResources(Device device)
		{
			ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
			SwapChainDescription swapChainDesc = new SwapChainDescription()
			{
				ModeDescription = backBufferDesc,
				SampleDescription = new SampleDescription(MSAASampleCount, 0),
				Usage = Usage.RenderTargetOutput,
				BufferCount = 1,
				SwapEffect = SwapEffect.Discard,
				OutputHandle = m_renderSurface.GetHWND(),
				IsWindowed = true
			};

			SwapChain?.Dispose();
			using (SharpDX.DXGI.Device dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
			{
				using (Adapter dxgiAdapter = dxgiDevice.Adapter)
				{
					using (Factory dxgiFactory = dxgiAdapter.GetParent<Factory>())
					{
						SwapChain = new SwapChain(dxgiFactory, device, swapChainDesc);
					}
				}
			}

			using (Texture2D backBuffer = SwapChain.GetBackBuffer<Texture2D>(0))
			{
				RenderTargetView?.Dispose();
				RenderTargetView = new RenderTargetView(device, backBuffer);
			}

			CreateDepthStencilView(Width, Height, device);
			InitializeDepthStencilState(device);
			InitializeRasterizer(device);
			Viewport = new Viewport(0, 0, Width, Height, 0.0f, 1.0f);
		}

		public void InitializeRasterizer(Device device)
		{
			RasterizerState oldState = RasterizerState;

			// TODO henning It should be possible to change the rasterizer state easily during the frame to render different objects with different parameters
			RasterizerStateDescription rasterizerStateDescription;
			rasterizerStateDescription.IsAntialiasedLineEnabled = false;
			rasterizerStateDescription.CullMode = CullMode.Back;
			rasterizerStateDescription.DepthBias = 1;
			rasterizerStateDescription.DepthBiasClamp = 0.0001f;
			rasterizerStateDescription.IsDepthClipEnabled = true;
			rasterizerStateDescription.FillMode = Wireframe ? FillMode.Wireframe : FillMode.Solid;
			rasterizerStateDescription.IsFrontCounterClockwise = false;
			rasterizerStateDescription.IsMultisampleEnabled = false;
			rasterizerStateDescription.IsScissorEnabled = false;
			rasterizerStateDescription.SlopeScaledDepthBias = 0.0001f;

			RasterizerState = new RasterizerState(device, rasterizerStateDescription);
			oldState?.Dispose();
		}

		private void CreateDepthStencilView(int width, int height, Device device)
		{
			// Create Depth Stencil
			Texture2DDescription depthTexDescription;
			depthTexDescription.Width = width;
			depthTexDescription.Height = height;
			depthTexDescription.MipLevels = 1;
			depthTexDescription.ArraySize = 1;
			depthTexDescription.Format = Format.D24_UNorm_S8_UInt;
			depthTexDescription.SampleDescription.Count = MSAASampleCount;
			depthTexDescription.SampleDescription.Quality = 0;
			depthTexDescription.Usage = ResourceUsage.Default;
			depthTexDescription.BindFlags = BindFlags.DepthStencil;
			depthTexDescription.CpuAccessFlags = CpuAccessFlags.None;
			depthTexDescription.OptionFlags = ResourceOptionFlags.None;

			DepthStencilTexture?.Dispose();
			DepthStencilTexture = new Texture2D(device, depthTexDescription);

			DepthStencilViewDescription depthViewDesc = new DepthStencilViewDescription
			{
				Format = Format.D24_UNorm_S8_UInt,
				Dimension = MSAASampleCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
				Texture2D = { MipSlice = 0 }
			};

			DepthStencilView?.Dispose();
			DepthStencilView = new DepthStencilView(device, DepthStencilTexture, depthViewDesc);
		}

		private void InitializeDepthStencilState(Device device)
		{
			DepthStencilStateDescription depthStencilDescription;
			depthStencilDescription.IsDepthEnabled = true;
			depthStencilDescription.DepthWriteMask = DepthWriteMask.All;
			depthStencilDescription.DepthComparison = Comparison.Less;

			depthStencilDescription.IsStencilEnabled = true;
			depthStencilDescription.StencilReadMask = 0xFF;
			depthStencilDescription.StencilWriteMask = 0xFF;

			depthStencilDescription.FrontFace.FailOperation = StencilOperation.Keep;
			depthStencilDescription.FrontFace.DepthFailOperation = StencilOperation.Keep;
			depthStencilDescription.FrontFace.PassOperation = StencilOperation.Keep;
			depthStencilDescription.FrontFace.Comparison = Comparison.Always;

			depthStencilDescription.BackFace.FailOperation = StencilOperation.Keep;
			depthStencilDescription.BackFace.DepthFailOperation = StencilOperation.Decrement;
			depthStencilDescription.BackFace.PassOperation = StencilOperation.Keep;
			depthStencilDescription.BackFace.Comparison = Comparison.Always;

			DepthStencilState?.Dispose();
			DepthStencilState = new DepthStencilState(device, depthStencilDescription);			
		}
		
		private IRenderSurface m_renderSurface;
		public IntPtr Hwnd
		{
			get { return m_renderSurface.GetHWND(); }
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Top { get; private set; }
		public int Left { get; private set; }

		public SwapChain SwapChain { get; private set; }
		public RenderTargetView RenderTargetView { get; private set; }
		public Texture2D DepthStencilTexture { get; private set; }
		public DepthStencilState DepthStencilState { get; private set; }
		public DepthStencilView DepthStencilView { get; private set; }
		public RasterizerState RasterizerState { get; private set; }
		public Viewport Viewport { get; private set; }
		public IntPtr UIContext { get; set; }
	}
}
