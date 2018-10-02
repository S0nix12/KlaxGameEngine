using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using SharpDX;
using SharpDX.D3DCompiler;

namespace KlaxCore.Core
{
    public class Game : IDisposable
    {
        public Game()
        {
            renderForm = new RenderForm("Game");
            renderForm.ClientSize = new Size(Width, Height);

            InitializeDeviceResources();
            InitShaders();
            InitMesh();            
        }

        public void Dispose()
        {
            renderForm.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderTargetView.Dispose();
            triangleVertexBuffer.Dispose();
            pixelShader.Dispose();
            vertexShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();          
            
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
            System.Threading.Thread.Sleep(10);
        }

        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);
            
            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };
            
            D3D11.Device.CreateWithSwapChain(DriverType.Hardware, D3D11.DeviceCreationFlags.None, swapChainDesc, out d3dDevice, out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
            {
                renderTargetView = new D3D11.RenderTargetView(d3dDevice, backBuffer);
            }

            viewport = new Viewport(0, 0, Width, Height);
            d3dDeviceContext.Rasterizer.SetViewport(viewport);
        }

        private void InitShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                vertexShader = new D3D11.VertexShader(d3dDevice, vertexShaderByteCode);
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new D3D11.PixelShader(d3dDevice, pixelShaderByteCode);
            }

            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            inputLayout = new D3D11.InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void InitMesh()
        {
            triangleVertexBuffer = D3D11.Buffer.Create<Vector3>(d3dDevice, D3D11.BindFlags.VertexBuffer, vertices);
        }

        private void Draw()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);

            red.Step();
            green.Step();
            blue.Step();

            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(red.GetCurrent(), green.GetCurrent(), blue.GetCurrent()));

            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            d3dDeviceContext.Draw(vertices.Count(), 0);


            swapChain.Present(1, PresentFlags.None);
        }

        private RenderForm renderForm;

        private const int Width = 1280;
        private const int Height = 720;

        private ValueEvolution<int> red = new ValueEvolution<int>(255, 0, 0, 3, 2);
        private ValueEvolution<int> green = new ValueEvolution<int>(255, 0, 0, 1, 4);
        private ValueEvolution<int> blue = new ValueEvolution<int>(255, 0, 0, 5, 3);

        private D3D11.Device d3dDevice;
        private D3D11.DeviceContext d3dDeviceContext;
        private SwapChain swapChain;
        private D3D11.RenderTargetView renderTargetView;

        private D3D11.Buffer triangleVertexBuffer;
        private Vector3[] vertices = new Vector3[] { new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f) };

        private D3D11.VertexShader vertexShader;
        private D3D11.PixelShader pixelShader;

        private D3D11.InputElement[] inputElements = new D3D11.InputElement[]
        {
                    new D3D11.InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
                    new D3D11.InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)
        };

        private ShaderSignature inputSignature;
        private D3D11.InputLayout inputLayout;
        private Viewport viewport;
    }
}
