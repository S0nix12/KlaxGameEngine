using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KlaxMath;
using KlaxRenderer.Camera;
using SharpDX;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Graphics
{
    class CMesh : IDrawable, System.IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        protected struct VertexType
        {
            public Vector3 position;
            public Vector4 color;
        };

        public void Init(Device device)
        {
            // Load resource
            // Create Buffers
            InitializeBuffers(device);
            Shader.Init(device);
        }

        public void Draw(DeviceContext deviceContext, ICamera camera)
        {
            // Draw Buffers
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, Utilities.SizeOf<VertexType>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            Shader.Render(deviceContext, IndexCount, Transform.GetTRSMatrix(), camera.ViewMatrix, camera.ProjectionMatrix);
        }

        public void Dispose()
        {
            m_indexBuffer.Dispose();
            m_vertexBuffer.Dispose();
        }

        protected virtual void InitializeBuffers(Device device)
        {
            throw new System.NotImplementedException("Not implemented for default mesh yet");
        }

        public Transform Transform
        { get; protected set; } = new Transform();

        // Shader to use when drawing this model, shader input Layout must match vertex Data Layout
        public IShader Shader
        { get; set; } = new CColorShader();
        protected int IndexCount
        { get; set; }
        protected int VertexCount
        { get; set; }
        protected Buffer m_indexBuffer;
        protected Buffer m_vertexBuffer;
    }
}
