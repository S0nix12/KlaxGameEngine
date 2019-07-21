using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Graphics;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene;
using KlaxShared.Attributes;
using KlaxShared.Containers;
using Newtonsoft.Json;

namespace KlaxCore.GameFramework
{
	[KlaxComponent(Category = "Visual")]
	class CModelComponent : CSceneComponent
	{
		public override void Init()
		{
			base.Init();
			if (Model != null)
			{
				if (Model.GetAsset() == null)
				{
					// Reference points to an invalid asset clear it
					Model = null;
				}
				else if (Model.GetAsset().IsLoaded)
				{
					RecreateRenderData(0);
				}
				else if (m_updateRenderDataScope == null || !m_updateRenderDataScope.IsConnected())
				{
					m_updateRenderDataScope = World.UpdateScheduler.Connect(RecreateRenderData, EUpdatePriority.ResourceLoading);
				}
			}
		}

		public override void Shutdown()
		{
			base.Shutdown();
			if (m_renderNodes != null)
			{
				CRenderScene scene = CRenderer.Instance.ActiveScene;
				foreach (CMeshRenderNode renderNode in m_renderNodes)
				{
					scene.UnregisterRenderNode(renderNode);
				}

				m_renderNodes = null;
			}

			if (m_updateRenderDataScope != null && m_updateRenderDataScope.IsConnected())
			{
				m_updateRenderDataScope.Disconnect();
				m_updateRenderDataScope = null;
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Makes the material at the given index unique")]
		public void CreateUniqueMaterial(int materialIndex)
		{
			if (m_renderNodes.Length < materialIndex)
			{
				m_renderNodes[materialIndex].CreateUniqueMaterial();
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the material at the given index")]
		public void SetMaterial(int materialIndex, CMaterial material)
		{
			m_overrideMaterials.SetMinSize(materialIndex + 1);
			m_overrideMaterials[materialIndex] = material;
			if (m_renderNodes.Length < materialIndex)
			{
				m_renderNodes[materialIndex].SetMaterialOverride(material);
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Get the material at the given index")]
		public CMaterial GetMaterial(int materialIndex)
		{
			if (m_renderNodes.Length < materialIndex)
			{
				return m_renderNodes[materialIndex].GetMaterial();
			}
			if (m_overrideMaterials.Count < materialIndex)
			{
				return m_overrideMaterials[materialIndex];
			}

			return null;
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Set the material at the given index from a material asset")]
		public void SetMaterialAsset(int materialIndex, CAssetReference<CMaterialAsset> materialAsset)
		{
			m_materialAssets.SetMinSize(materialIndex + 1);
			m_materialAssets[materialIndex] = materialAsset;
			if (m_renderNodes.Length < materialIndex)
			{
				m_renderNodes[materialIndex].SetMaterialOverride(materialAsset);
			}
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Get the material asset at the given index")]
		public CAssetReference<CMaterialAsset> GetMaterialAsset(int materialIndex)
		{
			if (m_materialAssets.Count < materialIndex)
			{
				return m_materialAssets[materialIndex];
			}

			return null;
		}

		[KlaxFunction(Category = "Rendering", Tooltip = "Get the number of meshes this model has")]
		public int GetNumMeshes()
		{
			return m_renderNodes.Length;
		}

		private void RecreateRenderData(float deltaTime)
		{
			if (Model == null)
			{
				CRenderScene scene = CRenderer.Instance.ActiveScene;
				for (int i = 0; i < m_renderNodes.Length; i++)
				{
					scene.UnregisterRenderNode(m_renderNodes[i]);
				}

				return;
			}

			CModelAsset modelAsset = Model.GetAsset();
			if (modelAsset != null && modelAsset.IsLoaded)
			{
				CRenderScene scene = CRenderer.Instance.ActiveScene;
				for (int i = 0; i < m_renderNodes.Length; i++)
				{
					scene.UnregisterRenderNode(m_renderNodes[i]);
				}

				int meshCount = modelAsset.MeshChildren.Count;
				m_renderNodes = new CMeshRenderNode[meshCount];
				m_overrideMaterials.SetMinSize(meshCount);
				m_materialAssets.SetMinSize(meshCount);
				for (int i = 0; i < meshCount; i++)
				{
					Transform childTransform = new Transform();
					childTransform.SetFromMatrix(modelAsset.MeshChildren[i].relativeTransform);
					childTransform.Parent = m_transform;
					CMeshRenderNode newRenderNode;
					if (m_overrideMaterials[i] != null)
					{
						newRenderNode = new CMeshRenderNode(this, modelAsset.MeshChildren[i].meshAsset, null, childTransform);
						newRenderNode.SetMaterialOverride(m_overrideMaterials[i]);
					}
					else
					{
						newRenderNode = new CMeshRenderNode(this, modelAsset.MeshChildren[i].meshAsset, m_materialAssets[i], childTransform);
					}
					m_renderNodes[i] = newRenderNode;
					scene.RegisterRenderNode(newRenderNode);
				}

				m_updateRenderDataScope?.Disconnect();
				m_updateRenderDataScope = null;
			}
		}	

		[JsonProperty]
		private CAssetReference<CModelAsset> m_model;
		[JsonIgnore]
		[KlaxProperty(Category = "Rendering")]
		public CAssetReference<CModelAsset> Model
		{
			get { return m_model; }
			set
			{
				if (value != m_model)
				{
					m_model = value;
					if (m_model == null || (m_model.GetAsset() != null && m_model.GetAsset().IsLoaded))
					{
						RecreateRenderData(0);
					}
					else if(m_updateRenderDataScope == null || !m_updateRenderDataScope.IsConnected())
					{
						m_updateRenderDataScope = World.UpdateScheduler.Connect(RecreateRenderData, EUpdatePriority.ResourceLoading);
					}
				}
			}
		}

		[JsonProperty]
		private readonly List<CAssetReference<CMaterialAsset>> m_materialAssets = new List<CAssetReference<CMaterialAsset>>();
		private CMeshRenderNode[] m_renderNodes = new CMeshRenderNode[0];
		private readonly List<CMaterial> m_overrideMaterials = new List<CMaterial>();
		private CUpdateScope m_updateRenderDataScope;
	}
}
