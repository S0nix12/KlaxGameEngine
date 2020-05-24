using System;
using KlaxRenderer.Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace KlaxRenderer.Debug.Primitives
{
	abstract class CDebugDrawPrimitive : IDisposable
	{
		public const int MAX_NUM_INSTANCES = 1000000;
		public void Init(Device device)
		{
			CreateBuffer(device);
		}

		public void Draw(DeviceContext deviceContext, PrimitivePerInstanceData[] instanceData, int count)
		{
			deviceContext.MapSubresource(m_instanceBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream dataStream);
			dataStream.WriteRange(instanceData, 0, count);
			deviceContext.UnmapSubresource(m_instanceBuffer, 0);

			VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[]
			{
				new VertexBufferBinding(m_vertexBuffer, Utilities.SizeOf<DebugVertexType>(), 0),
				new VertexBufferBinding(m_instanceBuffer, Utilities.SizeOf<PrimitivePerInstanceData>(), 0)
			};

			deviceContext.InputAssembler.SetVertexBuffers(0, vertexBufferBindings);
			deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, Format.R16_UInt, 0);

			deviceContext.DrawIndexedInstanced(m_indexCount, count, 0, 0, 0);
		}

		protected abstract void CreateBuffer(Device device);

		protected void CreateBufferFromFile(Device device, string fileName)
		{
			DebugVertexType[] vertices;
			UInt16[] indices;

			ModelLoader modelLoader = new ModelLoader(device);

			modelLoader.LoadPrimitveFromFile(fileName, out vertices, out indices);
			m_vertexCount = vertices.Length;
			m_indexCount = indices.Length;

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

			modelLoader.Dispose();
		}

		public virtual void Dispose()
		{
			Utilities.Dispose(ref m_vertexBuffer);
			Utilities.Dispose(ref m_indexBuffer);
			Utilities.Dispose(ref m_instanceBuffer);
		}

		protected Buffer m_vertexBuffer;
		protected Buffer m_indexBuffer;
		protected Buffer m_instanceBuffer;

		protected int m_vertexCount;
		protected int m_indexCount;
	}
}
