using System.Runtime.InteropServices;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxRenderer.Graphics.ResourceManagement;
using KlaxShared.Attributes;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace KlaxRenderer.Graphics
{
    public class CMesh : CResource
	{
        [StructLayout(LayoutKind.Sequential)]
        protected struct VertexType
        {
            public Vector3 position;
            public Vector4 color;
        };

		public CMesh()
		{
			m_sizePerVertex = Utilities.SizeOf<VertexType>();
			Material = CRenderer.Instance.ResourceManager.DefaultMaterial;
		}

		public virtual void Init(Device device)
        {
            // Load resource
            // Create Buffers
			InitializeBuffers(device);
		}

		internal override void InitFromAsset(Device device, CAsset asset)
		{
			CMeshAsset meshAsset = (CMeshAsset) asset;
			System.Diagnostics.Debug.Assert(meshAsset != null && meshAsset.IsLoaded);

			int sizePerVertex = Utilities.SizeOf<SVertexInfo>();
			m_sizePerVertex = sizePerVertex;
			m_primitiveTopology = meshAsset.PrimitiveTopology;
			m_vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, meshAsset.VertexData, sizePerVertex * meshAsset.VertexData.Length);
			m_vertexCount = meshAsset.VertexData.Length;
			m_primitiveCount = meshAsset.FaceCount;
			m_indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, meshAsset.IndexData);
			m_indexCount = meshAsset.IndexData.Length;

			BoundingBox = new BoundingBox(meshAsset.AABBMin, meshAsset.AABBMax);
			BoundingSphere = SharpDX.BoundingSphere.FromBox(BoundingBox);

			if (meshAsset.MaterialAsset != null)
			{
				Material = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CMaterial>(meshAsset.MaterialAsset);
			}
			else
			{
				Material = CRenderer.Instance.ResourceManager.DefaultMaterial;
			}
		}

		internal override void InitWithContext(Device device, DeviceContext deviceContext)
		{}

		internal override bool IsAssetCorrectType(CAsset asset)
		{
			return asset is CMeshAsset;
		}

		protected virtual void InitializeBuffers(Device device)
		{
			// Do nothing buffer will be provided from ModelLoader
		}

		public void Render(DeviceContext deviceContext)
        {
			Material.Render(deviceContext, Matrix.Multiply(Transform.WorldMatrix, ParentTransform.LocalMatrix));
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, m_sizePerVertex, 0));
			deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
			deviceContext.InputAssembler.PrimitiveTopology = m_primitiveTopology;
			deviceContext.DrawIndexed(m_indexCount, 0, 0);
        }

		public void Render(DeviceContext deviceContext, Transform transform)
		{
			Material.Render(deviceContext, transform.WorldMatrix);
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, m_sizePerVertex, 0));
			deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
			deviceContext.InputAssembler.PrimitiveTopology = m_primitiveTopology;
			deviceContext.DrawIndexed(m_indexCount, 0, 0);
		}

		public void RenderWithMaterial(DeviceContext deviceContext, CMaterial material)
		{
			System.Diagnostics.Debug.Assert(material != null);
			material.Render(deviceContext, Matrix.Multiply(Transform.WorldMatrix, ParentTransform.LocalMatrix));
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, m_sizePerVertex, 0));
			deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
			deviceContext.InputAssembler.PrimitiveTopology = m_primitiveTopology;
			deviceContext.DrawIndexed(m_indexCount, 0, 0);
		}
		public void RenderWithMaterial(DeviceContext deviceContext, CMaterial material, Transform transform)
		{
			System.Diagnostics.Debug.Assert(material != null);
			material.Render(deviceContext, transform.WorldMatrix);
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_vertexBuffer, m_sizePerVertex, 0));
			deviceContext.InputAssembler.SetIndexBuffer(m_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
			deviceContext.InputAssembler.PrimitiveTopology = m_primitiveTopology;
			deviceContext.DrawIndexed(m_indexCount, 0, 0);
		}

		public override void Dispose()
        {
			Material.Dispose();
			m_indexBuffer.Dispose();
			m_vertexBuffer.Dispose();
        }

		public void SetParent(Transform transform)
		{
			ParentTransform = transform;
		}

		public Transform Transform
        { get; protected set; } = new Transform();

		// Material to use when drawing this mesh, can be overriden by calling RenderWithMaterial
		public CMaterial Material { get; set; }
		public BoundingBox BoundingBox { get; protected set; }
		public BoundingSphere BoundingSphere { get; protected set; }
		protected Transform ParentTransform { get; set; } = new Transform();

		internal int m_indexCount;
		internal int m_vertexCount;
		internal int m_primitiveCount;
		internal PrimitiveTopology m_primitiveTopology = PrimitiveTopology.TriangleList;
		internal Buffer m_indexBuffer;
		internal Buffer m_vertexBuffer;
		internal int m_sizePerVertex;
	}
}
