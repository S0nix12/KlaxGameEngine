using KlaxRenderer.Graphics;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KlaxRenderer.Tests
{
    class CTestCube : CMesh
    {
        protected override void InitializeBuffers(Device device)
        {
            VertexType[] vertices = new VertexType[3];
            UInt32[] indices = new UInt32[3];

            VertexCount = 8;
            IndexCount = 3;

            vertices[0].position = new Vector3(-1.0f, -1.0f, 0.0f);
            vertices[0].color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

            vertices[1].position = new Vector3(-1.0f, 0.0f, 0.0f);
            vertices[1].color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

            vertices[2].position = new Vector3(0.0f, -1.0f, 0.0f);
            vertices[2].color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

            vertices[3].position = new Vector3(0.0f, 0.0f, 0.0f);
            vertices[3].color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

            vertices[4].position = new Vector3(-1.0f, -1.0f, -1.0f);
            vertices[4].color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

            vertices[5].position = new Vector3(-1.0f, 0.0f, -1.0f);
            vertices[5].color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

            vertices[6].position = new Vector3(0.0f, -1.0f, -1.0f);
            vertices[6].color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

            vertices[7].position = new Vector3(0.0f, 0.0f, -1.0f);
            vertices[7].color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;
            indices[6] = 2;
            indices[7] = 3;
            indices[8] = 6;

            BufferDescription vertexBufferDescription = new BufferDescription(Utilities.SizeOf<VertexType>() * VertexCount, BindFlags.VertexBuffer, ResourceUsage.Default);
            m_vertexBuffer = SharpDX.Direct3D11.Buffer.Create<VertexType>(device, vertices, vertexBufferDescription);
            m_indexBuffer = SharpDX.Direct3D11.Buffer.Create<UInt32>(device, BindFlags.IndexBuffer, indices);
        }
    }
}
