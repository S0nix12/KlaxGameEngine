using System.Collections.Generic;
using System.Linq;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxMath.Geometry;
using KlaxRenderer.Debug;
using KlaxRenderer.Graphics;
using KlaxShared;
using KlaxShared.Attributes;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace KlaxRenderer.RenderNodes
{
	public class CMeshRenderNode : CRenderNode
	{
		[CVar]
		public static int ShowBoundingBox { get; private set; } = 0;

		[CVar]
		public static int ShowOrientedBoundingBox { get; private set; } = 0;

		public CMeshRenderNode(object outer, CMeshAsset meshAsset, CMaterialAsset materialOverride, Transform transform) : base(outer)
		{
			m_sourceAsset = meshAsset;
			m_sourceOverrideMaterial = materialOverride;
			m_transform = transform;
		}

		public override void Dispose()
		{
			m_mesh.Dispose();
			m_overrideMaterial?.Dispose();
		}

		public override void Draw(DeviceContext deviceContext)
		{
			System.Diagnostics.Debug.Assert(IsFullyLoaded);
			if (ShowBoundingBox > 0)
			{
				BoundingBox transformedBox = m_mesh.BoundingBox.TransformBoundingBox(m_transform.WorldMatrix);
				CRenderer.Instance.ActiveScene.DebugRenderer.DrawBox(transformedBox.Center, Quaternion.Identity, transformedBox.Size, Color.Red.ToColor4(), 0.0f, EDebugDrawCommandFlags.Wireframe);
			}

			if (ShowOrientedBoundingBox > 0)
			{
				OrientedBoundingBox obb = new OrientedBoundingBox(m_mesh.BoundingBox);
				obb.Transform(m_transform.WorldMatrix);
				obb.Transformation.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
				CRenderer.Instance.ActiveScene.DebugRenderer.DrawBox(translation, rotation, obb.Size * scale, Color.Green.ToColor4(), 0.0f, EDebugDrawCommandFlags.Wireframe);
			}

			if (m_overrideMaterial != null)
			{
				if (m_overrideMaterial.IsLoaded)
				{
					m_mesh.RenderWithMaterial(deviceContext, m_overrideMaterial, m_transform);
				}
			}
			else
			{
				m_mesh.Render(deviceContext, m_transform);
			}
		}

		public override bool TryCreateResources()
		{
			if (!m_bIsLoading)
			{
				m_mesh = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CMesh>(m_sourceAsset);
				if (m_sourceOverrideMaterial != null)
				{
					m_overrideMaterial = CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CMaterial>(m_sourceOverrideMaterial);
				}

				if (!m_mesh.IsLoaded || !m_mesh.Material.IsLoaded || (m_overrideMaterial != null && !m_overrideMaterial.IsLoaded))
				{
					m_bIsLoading = true;
					return false;
				}

				FinishLoading();
				return true;
			}
			else
			{
				if (m_mesh.IsLoaded && m_mesh.Material.IsLoaded && (m_overrideMaterial == null || m_overrideMaterial.IsLoaded))
				{
					FinishLoading();
					return true;
				}

				return false;
			}
		}

		public override bool Intersects(Ray ray, out STriangle hitTriangle, out float hitDistance)
		{
			BoundingBox boundingBox = m_mesh.BoundingBox.TransformBoundingBox(m_transform.WorldMatrix);
			if (ray.Intersects(ref boundingBox, out hitDistance))
			{
				if (m_sourceAsset == null || m_sourceAsset.PrimitiveTopology != PrimitiveTopology.TriangleList)
				{
					hitTriangle = new STriangle();
					return false;
				}
			}
			else
			{
				hitTriangle = new STriangle();
				return false;
			}

			Matrix worldTransform = m_transform.WorldMatrix;
			var closestHit = (0, 1e10f);
			bool bTriangleHit = false;
			for (int i = 0; i < m_sourceAsset.IndexData.Length - 2; i += 3)
			{
				Vector3 vert1 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[i]].position, worldTransform);
				Vector3 vert2 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[i + 1]].position, worldTransform);
				Vector3 vert3 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[i + 2]].position, worldTransform);

				if (ray.Intersects(ref vert1, ref vert2, ref vert3, out float distance))
				{
					if (distance < closestHit.Item2)
					{
						closestHit = (i, distance);
						bTriangleHit = true;
					}
				}
			}

			if (!bTriangleHit)
			{
				hitTriangle = new STriangle();
				return false;
			}

			Vector3 p1 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[closestHit.Item1]].position, worldTransform);
			Vector3 p2 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[closestHit.Item1 + 1]].position, worldTransform);
			Vector3 p3 = (Vector3)Vector3.Transform(m_sourceAsset.VertexData[m_sourceAsset.IndexData[closestHit.Item1 + 2]].position, worldTransform);

			hitTriangle = new STriangle(p1, p2, p3);
			hitDistance = closestHit.Item2;

			return true;
		}

		public override ContainmentType FrustumTest(in BoundingFrustum frustum)
		{
			BoundingBox boundingBox = m_mesh.BoundingBox.TransformBoundingBox(m_transform.WorldMatrix);
			return frustum.Contains(ref boundingBox);
		}

		private void FinishLoading()
		{
			IsFullyLoaded = true;
			if (HasUniqueMaterial)
			{
				SetUniqueMaterial();
			}

			if (m_placeholderMaterial.GetNumActiveParameters() > 0)
			{
				if (m_overrideMaterial != null)
				{
					m_overrideMaterial.MergeWith(m_placeholderMaterial);
					m_placeholderMaterial.MergeWith(m_overrideMaterial);
					m_placeholderMaterial = null;
				}
				else
				{
					m_mesh.Material.MergeWith(m_placeholderMaterial);
					m_placeholderMaterial.MergeWith(m_mesh.Material);
					m_placeholderMaterial = null;
				}
			}
		}

		public void CreateUniqueMaterial()
		{
			if (!HasUniqueMaterial)
			{
				if (IsFullyLoaded)
				{
					SetUniqueMaterial();
				}
				HasUniqueMaterial = true;
			}
		}

		private void SetUniqueMaterial()
		{
			CMaterial uniqueMaterial = new CMaterial();
			if (m_overrideMaterial != null)
			{
				uniqueMaterial.ShaderResource = m_overrideMaterial.ShaderResource;
				uniqueMaterial.MergeWith(m_overrideMaterial);
			}
			else
			{
				uniqueMaterial.ShaderResource = m_mesh.Material.ShaderResource;
				uniqueMaterial.MergeWith(m_mesh.Material);
			}
			uniqueMaterial.FinishLoading();
			m_overrideMaterial = uniqueMaterial;
		}

		public CMaterial GetMaterial()
		{
			if (m_overrideMaterial != null)
			{
				return m_overrideMaterial;
			}

			if (m_mesh != null && m_mesh.IsLoaded && m_mesh.Material != null)
			{
				return m_mesh.Material;
			}

			return m_placeholderMaterial;
		}

		public CMaterial GetOverrideMaterial()
		{
			return m_overrideMaterial;
		}

		public void SetMaterialOverride(CMaterialAsset materialAsset)
		{
			m_sourceOverrideMaterial = null;
			m_overrideMaterial = materialAsset != null ? CRenderer.Instance.ResourceManager.RequestResourceFromAsset<CMaterial>(materialAsset) : null;
		}

		public void SetMaterialOverride(CMaterial overrideMaterial)
		{
			m_sourceOverrideMaterial = null;
			m_overrideMaterial = overrideMaterial;
		}

		public bool HasUniqueMaterial { get; internal set; }

		internal CMesh m_mesh;
		internal CMaterial m_overrideMaterial;
		internal Transform m_transform;

		// Source Assets will be null after the render resources are created

		// We cache material parameter settings during the loading process so they can be applied after loading is finished
		private CMaterial m_placeholderMaterial = new CMaterial();
		
		private bool m_bIsLoading;
		internal CMeshAsset m_sourceAsset;
		private CMaterialAsset m_sourceOverrideMaterial;
	}
}
