using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.D3DCompiler;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Graphics
{
    class CD3DRenderer : IDisposable
    {
        public delegate void DrawCallback(DeviceContext deviceContext);

        public CD3DRenderer()
        {}

        public void Init(string windowName, int width, int height)
        {
            m_width = width;
            m_height = height;
            m_renderForm = new RenderForm(windowName);
            m_renderForm.ClientSize = new Size(m_width, m_height);

            Init();
        }

        public void Init(RenderForm window)
        {
            m_renderForm = window;
            m_width = window.ClientSize.Width;
            m_height = window.ClientSize.Height;

            Init();
        }

        private void Init()
        {
            InitializeDeviceResources();
        }
        
        public void Dispose()
        {
            m_renderForm.Dispose();
            m_swapChain.Dispose();
            m_d3dDevice.Dispose();
            m_d3dDeviceContext.Dispose();
            m_renderTargetView.Dispose();

        }

        public void Run(DrawCallback inDrawCallback)
        {
            m_drawCallback = inDrawCallback;
            RenderLoop.Run(m_renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            BeginRender();
            m_drawCallback(m_d3dDeviceContext);
            System.Threading.Thread.Sleep(30); // Work
            EndRender();
        }

        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(m_width, m_height, new Rational(60, 1), Format.R8G8B8A8_UNorm);                        
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = m_renderForm.Handle,
                IsWindowed = true
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out m_d3dDevice, out m_swapChain);
            m_d3dDeviceContext = m_d3dDevice.ImmediateContext;

            using (Texture2D backBuffer = m_swapChain.GetBackBuffer<Texture2D>(0))
            {
                m_renderTargetView = new RenderTargetView(m_d3dDevice, backBuffer);
            }

            m_viewport = new Viewport(0, 0, m_width, m_height);
            m_d3dDeviceContext.Rasterizer.SetViewport(m_viewport);
        }

        private void BeginRender()
        {
            m_d3dDeviceContext.OutputMerger.SetRenderTargets(m_renderTargetView);
            m_d3dDeviceContext.ClearRenderTargetView(m_renderTargetView, ClearColor);            
        }

        private void EndRender()
        {
            m_swapChain.Present(EnableVsync ? 1 : 0, PresentFlags.None);
        }

        public SharpDX.Color ClearColor
        { get; set; } = new SharpDX.Color(255, 0, 0);

        public bool EnableVsync
        { get; set; } = true;

        public Device D3DDevie
        { get { return m_d3dDevice; } }

        public DeviceContext D3DDeviceContext
        { get { return m_d3dDeviceContext; } }

        private RenderForm m_renderForm;

        private int m_width;
        private int m_height;

        private Device m_d3dDevice;
        private DeviceContext m_d3dDeviceContext;
        private SwapChain m_swapChain;
        private RenderTargetView m_renderTargetView;
        private DrawCallback m_drawCallback;

        private Viewport m_viewport;
    }
}
