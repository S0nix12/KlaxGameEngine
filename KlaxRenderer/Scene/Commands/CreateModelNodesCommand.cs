using KlaxIO.AssetManager.Assets;
using KlaxMath;
using KlaxRenderer.RenderNodes;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Scene.Commands
{
	//todo henning support override material for child meshes
	public class CCreateModelNodesCommand : IRenderSceneCommand
	{
		private CCreateModelNodesCommand()
		{}

		public CCreateModelNodesCommand(object outer, CModelAsset modelAsset, Transform modelTransform)
		{
			m_outer = outer;
			m_sourceAsset = modelAsset;
			m_modelTransform = modelTransform;
		}

		public bool TryExecute(Device device, DeviceContext deviceContext, CRenderScene scene)
		{
			if (!m_sourceAsset.IsLoaded)
			{
				return false;
			}

			for (int i = 0; i < m_sourceAsset.MeshChildren.Count; i++)
			{
				SMeshChild meshChild = m_sourceAsset.MeshChildren[i];
				Transform childTransform = new Transform();
				childTransform.SetFromMatrix(in meshChild.relativeTransform);
				childTransform.Parent = m_modelTransform;

				CMeshRenderNode renderNode = new CMeshRenderNode(m_outer, meshChild.meshAsset, null, childTransform);
				if (meshChild.meshAsset.GetAsset().IsLoaded)
				{
					renderNode.TryCreateResources();
				}
				scene.RegisterRenderNode(renderNode);
			}

			return true;
		}

		private object m_outer;
		private CModelAsset m_sourceAsset;
		private Transform m_modelTransform;
	}
}
