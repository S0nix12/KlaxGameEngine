using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    // TODO test Code replace with precaluclated WorldViewProjection Matrix
    [StructLayout(LayoutKind.Sequential)]
    struct MatrixBufferType
    {
        Matrix world;
        Matrix view;
        Matrix projection;

        public static int SizeInBytes()
        {
            return Matrix.SizeInBytes * 3;
        }
    }

    class CColorShader : IShader
    {
        public bool Init(Device device)
        {
            return InitializeShader(device);
        }

        public void Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            SetShaderParameters(deviceContext, ref worldMatrix, ref viewMatrix, ref projectionMatrix);
            RenderShader(deviceContext, indexCount);
        }

        public void Shutdown()
        {
            m_matrixBuffer.Dispose();
            m_pixelShader.Dispose();
            m_vertexShader.Dispose();
            m_inputLayout.Dispose();
        }

        private bool InitializeShader(Device device)
        {
            // TODO henning replace with proper shader loading and offline compile
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/vertexShader.hlsl", "main", "vs_4_0", ShaderFlags.Debug))
            {
                if(vertexShaderByteCode.HasErrors)
                {
                    throw new Exception(vertexShaderByteCode.Message);
                }
                m_vertexShader = new VertexShader(device, vertexShaderByteCode);                
                ShaderSignature inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                InputElement[] inputElements = new InputElement[]
                {
                    new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0),
                    new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0)
                };

                m_inputLayout = new InputLayout(device, inputSignature, inputElements);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders/pixelShader.hlsl", "main", "ps_4_0", ShaderFlags.Debug))
            {
                if(pixelShaderByteCode.HasErrors)
                {
                    throw new Exception(pixelShaderByteCode.Message);
                }
                m_pixelShader = new PixelShader(device, pixelShaderByteCode);
            }

            BufferDescription bufferDescription = new BufferDescription(MatrixBufferType.SizeInBytes(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            m_matrixBuffer = new SharpDX.Direct3D11.Buffer(device, bufferDescription);
            return true;
        }

        private void SetShaderParameters(DeviceContext deviceContext, ref Matrix worldMatrix, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            DataStream shaderStream;
            DataBox data = deviceContext.MapSubresource(m_matrixBuffer, MapMode.WriteDiscard, MapFlags.None, out shaderStream);
            WriteMatrixToShaderBufferStream(shaderStream, ref worldMatrix);
            WriteMatrixToShaderBufferStream(shaderStream, ref viewMatrix);
            WriteMatrixToShaderBufferStream(shaderStream, ref projectionMatrix);
            deviceContext.UnmapSubresource(m_matrixBuffer, 0);
            shaderStream.Dispose();

            deviceContext.VertexShader.SetConstantBuffer(0, m_matrixBuffer);
        }

        private static void WriteMatrixToShaderBufferStream(DataStream shaderStream, ref Matrix value)
        {
            shaderStream.Write(Matrix.Transpose(value));
        }

        private void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            // Set vertex shader input layout
            deviceContext.InputAssembler.InputLayout = m_inputLayout;

            // Set vertex and pixel shader active to render objects with this shader
            deviceContext.VertexShader.Set(m_vertexShader);
            deviceContext.PixelShader.Set(m_pixelShader);

            deviceContext.DrawIndexed(indexCount, 0, 0);
            //deviceContext.Draw(3, 0);
        }

        private VertexShader m_vertexShader;
        private PixelShader m_pixelShader;
        private InputLayout m_inputLayout;
        private SharpDX.Direct3D11.Buffer m_matrixBuffer;
    }
}
