using System;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace KlaxRenderer.Debug.Primitives
{
	class CDebugCube : CDebugDrawPrimitive
	{
		protected override void CreateBuffer(Device device)
		{
			m_vertexCount = 8;
			m_indexCount = 36;

			DebugVertexType[] vertices = new DebugVertexType[m_vertexCount];
			UInt16[] indices = new UInt16[m_indexCount];

			vertices[0].position = new Vector3(-0.5f, -0.5f, -0.5f);
			vertices[1].position = new Vector3(-0.5f, 0.5f, -0.5f);
			vertices[2].position = new Vector3(0.5f, -0.5f, -0.5f);
			vertices[3].position = new Vector3(0.5f, 0.5f, -0.5f);
			vertices[4].position = new Vector3(-0.5f, -0.5f, 0.5f);
			vertices[5].position = new Vector3(-0.5f, 0.5f, 0.5f);
			vertices[6].position = new Vector3(0.5f, -0.5f, 0.5f);
			vertices[7].position = new Vector3(0.5f, 0.5f, 0.5f);

			indices[0] = 0;
			indices[1] = 1;
			indices[2] = 2;
			indices[3] = 1;
			indices[4] = 3;
			indices[5] = 2;
			indices[6] = 2;
			indices[7] = 3;
			indices[8] = 6;
			indices[9] = 6;
			indices[10] = 3;
			indices[11] = 7;
			indices[12] = 1;
			indices[13] = 5;
			indices[14] = 3;
			indices[15] = 5;
			indices[16] = 7;
			indices[17] = 3;
			indices[18] = 5;
			indices[19] = 6;
			indices[20] = 7;
			indices[21] = 6;
			indices[22] = 5;
			indices[23] = 4;
			indices[24] = 4;
			indices[25] = 1;
			indices[26] = 0;
			indices[27] = 1;
			indices[28] = 4;
			indices[29] = 5;
			indices[30] = 0;
			indices[31] = 2;
			indices[32] = 4;
			indices[33] = 6;
			indices[34] = 4;
			indices[35] = 2;

			BufferDescription vertexBufferDescription = new BufferDescription(Utilities.SizeOf<DebugVertexType>() * m_vertexCount, BindFlags.VertexBuffer, ResourceUsage.Default);
			m_vertexBuffer = Buffer.Create(device, vertices, vertexBufferDescription);
			m_indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

			BufferDescription instanceBufferDesc = new BufferDescription()
			{
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = Utilities.SizeOf<PrimitivePerInstanceData>() * MAX_NUM_INSTANCES,
				Usage = ResourceUsage.Dynamic
			};

			m_instanceBuffer = new Buffer(device, instanceBufferDesc);
		}
	}
}
