using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlaxCore.Core;
using KlaxIO.AssetManager.Assets;
using KlaxRenderer;
using KlaxRenderer.RenderNodes;
using KlaxRenderer.Scene.Commands;
using KlaxShared;
using KlaxShared.Attributes;
using KlaxShared.Definitions.Graphics;
using SharpDX;

namespace KlaxCore.GameFramework
{
	class CGameMaterial : CWorldObject
    {
	    public void SetTargetNode(CMeshRenderNode targetNode)
	    {
		    m_targetRenderNode = targetNode;
			IsUniqueMaterial = targetNode.HasUniqueMaterial;
			if (m_parametersToUpdate.Count > 0)
			{
				ConnectRenderParamUpdate();
			}
		}

		public void CreateUniqueMaterial()
		{
			if (!IsUniqueMaterial && !m_bCreateUnique)
			{
				IsUniqueMaterial = true;
				m_bCreateUnique = true;
				ConnectRenderParamUpdate();
			}
		}

		[KlaxFunction]
		public void SetScalarParameter(SHashedName parameterName, float scalar)
		{
			UpdateParameter(parameterName, EShaderParameterType.Scalar, scalar);
		}

		[KlaxFunction]
		public void SetVectorParameter(SHashedName parameterName, in Vector3 vector)
		{
			UpdateParameter(parameterName, EShaderParameterType.Vector, vector);
		}

		[KlaxFunction]
		public void SetColorParameter(SHashedName parameterName, in Vector4 color)
		{
			UpdateParameter(parameterName, EShaderParameterType.Color, color);
		}

		[KlaxFunction]
		public void SetMatrixParameter(SHashedName parameterName, in Matrix matrix)
		{
			UpdateParameter(parameterName, EShaderParameterType.Matrix, matrix);
		}

		[KlaxFunction]
		public void SetTextureParameter(SHashedName parameterName, CTextureAsset texture)
		{
			UpdateParameter(parameterName, EShaderParameterType.Texture, texture);
		}

		private void UpdateRenderParameters(float deltaTime)
	    {
			if (CRenderer.UseRenderThread > 0)
			{
				if (m_bCreateUnique)
				{
					CCreateUniqueMaterialCommand uniqueMaterialCommand = new CCreateUniqueMaterialCommand(m_targetRenderNode);
					CRenderer.Instance.ActiveScene.AddCommand(uniqueMaterialCommand);
				}

				if (m_parametersToUpdate.Count > 0)
				{
					CSetMaterialParametersCommand materialParametersCommand = new CSetMaterialParametersCommand(m_targetRenderNode, m_parametersToUpdate);
					CRenderer.Instance.ActiveScene.AddCommand(materialParametersCommand);
				}
			}
			else
			{
				if (m_bCreateUnique)
				{
					m_targetRenderNode.CreateUniqueMaterial();
					m_bCreateUnique = false;
				}
			}
			m_parametersToUpdate.Clear();
	    }

	    private void UpdateParameter(SHashedName name, EShaderParameterType type, object data)
	    {
		    SMaterialParameterEntry parameterEntry = new SMaterialParameterEntry(name, new SShaderParameter(type, data));
			m_parametersToUpdate.Add(parameterEntry);

		    ConnectRenderParamUpdate();
		}
		private void ConnectRenderParamUpdate()
		{
			if (m_targetRenderNode != null && (m_parameterUpdateScope == null || !m_parameterUpdateScope.IsConnected()))
			{
				m_parameterUpdateScope = World.UpdateScheduler.ConnectOneTimeUpdate(UpdateRenderParameters, EUpdatePriority.ResourceLoading);
			}
		}

		public bool IsUniqueMaterial { get; protected set; }

        private CMeshRenderNode m_targetRenderNode;
        private readonly List<SMaterialParameterEntry> m_parametersToUpdate  = new List<SMaterialParameterEntry>();
		private bool m_bCreateUnique;
	    private CUpdateScope m_parameterUpdateScope;
    }
}
