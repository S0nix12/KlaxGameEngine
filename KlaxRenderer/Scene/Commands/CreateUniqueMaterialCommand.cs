using KlaxRenderer.RenderNodes;
using SharpDX.Direct3D11;

namespace KlaxRenderer.Scene.Commands
{
	public class CCreateUniqueMaterialCommand : IRenderSceneCommand
	{
		public CCreateUniqueMaterialCommand(CMeshRenderNode targetNode)
		{
			m_targetNode = targetNode;
		}

		public bool TryExecute(Device device, DeviceContext deviceContext, CRenderScene scene)
		{
			m_targetNode.CreateUniqueMaterial();
			return true;
		}

		private readonly CMeshRenderNode m_targetNode;
	}
}
