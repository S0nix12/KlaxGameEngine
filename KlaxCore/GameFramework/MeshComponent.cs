using KlaxIO.AssetManager.Assets;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using KlaxShared.Attributes;
using Newtonsoft.Json;

namespace KlaxCore.GameFramework
{
	[KlaxComponent(Category = "Visual")]
	public class CMeshComponent : CSceneComponent
	{
		public override void Init()
		{
			base.Init();
			RecreateRenderData();
		}

		public override void Shutdown()
		{
			base.Shutdown();
			if (m_renderNode != null)
			{
				CRenderer.Instance.ActiveScene.UnregisterRenderNode(m_renderNode);
				m_renderNode = null;
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Makes the material of this mesh component unique")]
		public void CreateUniqueMaterial()
		{
			m_renderNode?.CreateUniqueMaterial();
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Get the material assigned to this mesh component")]
		public CMaterial GetMaterial()
		{
			if (m_renderNode != null)
			{
				return m_renderNode.GetMaterial();
			}
			else
			{
				return m_overrideMaterial;
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the material assigned to this mesh component")]
		public void SetMaterial(CMaterial material)
		{
			m_overrideMaterial = material;
			m_renderNode?.SetMaterialOverride(material);
		}

		private void RecreateRenderData()
		{
			if (Mesh?.GetAsset() != null)
			{
				CRenderScene scene = CRenderer.Instance.ActiveScene;
				if (m_renderNode != null)
				{
					scene.UnregisterRenderNode(m_renderNode);
				}

				if (m_overrideMaterial != null)
				{
					m_renderNode = new CMeshRenderNode(this, Mesh, null, m_transform);
					m_renderNode.SetMaterialOverride(m_overrideMaterial);
				}
				else
				{
					m_renderNode = new CMeshRenderNode(this, Mesh, MaterialAsset, m_transform);
				}
				scene.RegisterRenderNode(m_renderNode);
			}
			else
			{
				if (m_renderNode != null)
				{
					CRenderer.Instance.ActiveScene.UnregisterRenderNode(m_renderNode);
				}
			}
		}

		private CAssetReference<CMaterialAsset> m_materialAsset;
		[KlaxProperty(Category = "Rendering", DisplayName = "Material")]
		public CAssetReference<CMaterialAsset> MaterialAsset
		{
			get { return m_materialAsset; }
			set
			{
				if (value != m_materialAsset)
				{
					m_materialAsset = value;
					m_renderNode?.SetMaterialOverride(m_materialAsset);
				}
			}
		}

		private CMaterial m_overrideMaterial;

		[JsonProperty]
		private CAssetReference<CMeshAsset> m_mesh;
		[JsonIgnore]
		[KlaxProperty(Category = "Rendering")]
		public CAssetReference<CMeshAsset> Mesh
		{
			get { return m_mesh; }
			set
			{
				if (value != m_mesh)
				{
					m_mesh = value;

					if (World != null)
					{
						RecreateRenderData();
					}
				}
			}
		}

		private CMeshRenderNode m_renderNode;
	}
}
